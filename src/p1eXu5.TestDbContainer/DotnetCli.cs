using System.Diagnostics;
using System.Net;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Models;
using p1eXu5.TestDbContainer.Options;

namespace TestDbContainer;

internal sealed class DotnetCli : IDotnetCli
{
    private static LocalIP? _localIP;

    public static LocalIP LocalIP
    {
        get
        {
            if (_localIP is null)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                _localIP =
                    new LocalIP(
                        host.AddressList
                            .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(a => a.ToString())
                            .First(s => s.StartsWith("192", StringComparison.Ordinal)));
            }

            return _localIP;
        }
    }

    /// <summary>
    /// Executes: <code>dotnet ef database update ...</code>
    /// </summary>
    /// <param name="testDb"></param>
    /// <returns></returns>
    public int UpdateDatabase(TestDbOptionsBase testDb, Func<LocalIP, string> connectionString)
    {
        return RunProcess(
            "dotnet",
            "ef",
            "database",
            "update",
            "--connection", $"{connectionString(LocalIP)}",
            "-p", $"{testDb.MigrationProjectPath}",
            "-s", $"{testDb.StartupProjectPath}");
    }

    public int CreateInitialMigration(TestDbOptionsBase testDb)
    {
        return RunProcess(
            "dotnet",
            "ef",
            "migrations",
            "add",
            "Init",
            "-p", $"{testDb.MigrationProjectPath}",
            "-s", $"{testDb.StartupProjectPath}");
    }

    public int Compose(ComposeVerb composeVerb)
    {
        return RunProcess(
            "docker",
            "compose",
            "-f", $"{composeVerb.ComposeFile}",
            "up",
            "-d",
            composeVerb.ContainerName);
    }

    private static int RunProcess(string fileName, params string[] argumentList)
    {
        var pi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        foreach (var arg in argumentList)
        {
            pi.ArgumentList.Add(arg);
        }

        var p = new Process { StartInfo = pi };
        p.OutputDataReceived += ProcessOutputDataReceived;
        p.ErrorDataReceived += ProcessErrorDataReceived;
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();

        p.OutputDataReceived -= ProcessOutputDataReceived;
        p.ErrorDataReceived -= ProcessErrorDataReceived;

        return p.ExitCode;
    }

    private static void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data);
    }

    private static void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data);
    }
}

using System.Diagnostics;
using System.Net;
using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal sealed class DotnetCli : IDotnetCli
{
    private static string? _localIp;

    public static string LocalIp
    {
        get
        {
            if (_localIp is null)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                _localIp =
                    host.AddressList
                        .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(a => a.ToString())
                        .First(s => s.StartsWith("192", StringComparison.Ordinal));
            }

            return _localIp;
        }
    }

    public static string ConnectionString(TestDbOptions testDb)
    {
        return $"server={LocalIp};port={testDb.ContainerExternalPort};uid=admin;pwd=admin;database={testDb.DatabaseName}";
    }

    public int UpdateDatabase(TestDbOptions testDb)
    {
        var pi = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            ArgumentList =
            {
                "ef",
                "database",
                "update",
                "-p",
                $"{testDb.MigrationProjectPath}",
                "-s",
                $"{testDb.StartupProjectPath}",
                "--connection",
                $"{ConnectionString(testDb)}",
            }
        };


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

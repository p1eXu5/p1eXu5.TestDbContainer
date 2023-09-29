using System.Collections.ObjectModel;
using System.Text;

namespace p1eXu5.TestDbContainer.Models;

public sealed record ComposeFile
{
    public string Name { get; init; } = default!;

    public Dictionary<string, Service> Services { get; init; } = default!;

    internal string MySqlConnectionString(LocalIP localIP, string service)
    {
        return new StringBuilder()
            .Append("server=").Append(localIP.Value)
            .Append(";port=").Append(Services[service].Ports.First().Split(':').First())
            .Append(";uid=").Append(Services[service].Environment["MYSQL_USER"])
            .Append(";pwd=").Append(Services[service].Environment["MYSQL_PASSWORD"])
            .Append(";database=").Append(Services[service].Environment["MYSQL_DATABASE"])
            .ToString();
    }

    internal bool ServiceExists(string serviceName)
        => Services.ContainsKey(serviceName);
}

public sealed record Service
{
    public Dictionary<string, string> Environment { get; init; } = default!;

    public Collection<string> Ports { get; init; } = default!;
}

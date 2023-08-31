using System.Reflection;

namespace TestDbContainer;

internal static class Paths
{
    static Paths()
    {
        var utils = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..\\..\\..\\..\\");
        Dbs = Path.Combine(utils, ".dbs");
        CoreDomainMigrationsProject = Path.Combine(utils, "..\\src\\DrugRoom.CoreDomain.Adapters.Persistence\\");
        CoreDomainStartupProject = Path.Combine(utils, "..\\src\\DrugRoom.CoreDomain.WebApi\\");
        CoreDomainMigrations = Path.Combine(utils, "..\\src\\DrugRoom.CoreDomain.Adapters.Persistence\\Migrations");
    }

    public static string Dbs { get; }

    public static string CoreDomainMigrations { get; }
    public static string CoreDomainMigrationsProject { get; }
    public static string CoreDomainStartupProject { get; }
}

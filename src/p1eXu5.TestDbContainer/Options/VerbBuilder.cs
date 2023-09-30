using p1eXu5.Result;
using p1eXu5.Result.Extensions;

namespace p1eXu5.TestDbContainer.Options;

internal class VerbBuilder
{
    public Result<TVerb, string> BuildVerb<TVerb>(TVerb options)
        where TVerb : TestDbOptionsBase
    {
        return
            ConfirmProject(options)
            .Bind(ConfirmMigrationsFolder);
    }

    protected virtual Result<TVerb, string> ConfirmProject<TVerb>(TVerb options)
        where TVerb : TestDbOptionsBase
    {
        return
            FindProjectFile(options.ProjectPath)
            .Map(path => options with { ProjectPath = path });
    }

    protected virtual Result<TVerb, string> ConfirmMigrationsFolder<TVerb>(TVerb options)
        where TVerb : TestDbOptionsBase
    {
        var migrationsPath =
            options.MigrationPathIsSet
                ? options.MigrationPath
                : Path.Combine(options.ProjectPath, "Migrations");

        if (Directory.Exists(migrationsPath))
        {
            return options.ToOkWithStringError();
        }

        return "Migrations folder path is not found".ToError<TVerb>();
    }

    protected virtual Result<TVerb, string> ConfirmStartupProjectFolder<TVerb>(TVerb options)
        where TVerb : TestDbOptionsBase
    {
        if (!options.StartupProjectPathIsSet)
        {
            return (options with { StartupProjectPath = options.ProjectPath }).ToOkWithStringError();
        }

        return
            FindProjectFile(options.StartupProjectPath!)
            .Map(path => options with { StartupProjectPath = path });
    }

    private static Result<string, string> FindProjectFile(string path)
    {
        if (File.Exists(path))
        {
            return path.ToOkWithStringError();
        }

        if (Directory.Exists(path))
        {
            var fileName = Path.Combine(path, Path.GetFileName(path) + ".csproj");
            if (File.Exists(fileName))
            {
                return fileName.ToOkWithStringError();
            }

            return $"Could not find project file in {path}".ToError<string>();
        }

        var projectFiles =
            Directory
                .EnumerateFiles(
        ".",
                    $"*{path}.csproj",
                    new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 6 })
                .ToArray();

        if (projectFiles.Length == 0)
        {
            return $"Could not find project with name {path}".ToError<string>();
        }

        if (projectFiles.Length > 1)
        {
            return $"There are several projects with name {path}. Need to refine project name.".ToError<string>();
        }

        return projectFiles[0].ToOkWithStringError();
    }
}

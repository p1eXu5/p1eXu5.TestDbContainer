using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Tests.Integration;

internal class VerbBuilderDecorator : VerbBuilder
{
    internal bool ProjectExists { get; set; } = true;

    internal bool MigrationsFolderExists { get; set; } = true;

    protected override Result<TVerb, string> ConfirmProject<TVerb>(TVerb options)
    {
        if (ProjectExists)
        {
            return options.ToOkWithStringError();
        }

        return "Project does not exists".ToError<TVerb>();
    }

    protected override Result<TVerb, string> ConfirmMigrationsFolder<TVerb>(TVerb options)
    {
        if (MigrationsFolderExists)
        {
            return
                options.MigrationPathIsSet
                    ? options.ToOkWithStringError()
                    : (options with { MigrationPath = "TestMigrations" }).ToOkWithStringError();
        }

        return "Migrations folder path does not exists".ToError<TVerb>();
    }
}

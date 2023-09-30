using p1eXu5.CliBootstrap;
using p1eXu5.TestDbContainer.Handlers;
using p1eXu5.TestDbContainer.Interfaces;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Tests.Integration;

public sealed partial class Sut
{
    private TestDbContainerBootstrapDecorator _bootstrap;

    internal IDbContext MockDbContext => _bootstrap.MockDbContext;
    internal IDockerContainer MockDockerContainer => _bootstrap.MockDockerContainer;
    internal IDotnetCli MockDotnetCli => _bootstrap.MockDotnetCli;
    internal IMigrationFolder MockPhysicalDirectory => _bootstrap.MockPhysicalDirectory;

    internal IOptionsHandler? OptionsHandler { get; private set; }

    internal TestDbContainerBootstrapDecorator Bootstrap
        => _bootstrap ??= new TestDbContainerBootstrapDecorator();

    internal void ProjectExists() => Bootstrap.VerbBuilderDecorator.ProjectExists = true;

    internal void MigrationFolderExists() => Bootstrap.VerbBuilderDecorator.MigrationsFolderExists = true;

    internal Task RunAsync(params string[] args)
    {
        Bootstrap.TestContextWriters.Out = TestContext.Out;
        Bootstrap.TestContextWriters.Progress = TestContext.Progress;
        return Bootstrap.RunAsync(args);
    }

    internal void ReplaceOptionsHandler()
    {
        var optionsHandler = Substitute.For<IOptionsHandler>();
        optionsHandler.HandleAsync(default!, default)
            .Returns(Task.CompletedTask);

        Bootstrap.ReplaceService<IOptionsHandler, OptionsHandler>(optionsHandler);

        OptionsHandler = optionsHandler;
    }
}

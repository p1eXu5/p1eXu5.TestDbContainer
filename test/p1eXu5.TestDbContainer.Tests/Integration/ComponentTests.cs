namespace p1eXu5.TestDbContainer.Tests.Integration;

public sealed class ComponentTests : TestsBase
{
    [Test]
    public async Task FulfilledContainerVerbOptions_ExecutesContainerHandler()
    {
        // Arrange:
        string[] args =
        {
            "container",
            "-m", "foo",
            "-p", "bar",
            "-s", "baz",
            "-c", "qux",
            "-e", "5000",
            "-n", "corge",
        };

        Sut.ReplaceOptionsHandler();

        // Action:
        await Sut.RunAsync(args);

        // Assert:
        await Assert.OptionsHandlerReceivedContainerVerb();
    }

    [Test]
    public async Task FulfilledDefaultContainerVerbOptions_ExecutesContainerHandler()
    {
        // Arrange:
        string[] args =
        {
            "-m", "foo",
            "-p", "bar",
            "-s", "baz",
            "-c", "qux",
            "-e", "5000",
            "-n", "corge",
        };

        Sut.ReplaceOptionsHandler();

        // Action:
        await Sut.RunAsync(args);

        // Assert:
        await Assert.OptionsHandlerReceivedContainerVerb();
    }

    [Test]
    public async Task ArgumentsContainsProjectNameAndPort_ExecutesContainerHandler()
    {
        // Arrange:
        Sut.ProjectExists();
        Sut.MigrationFolderExists();

        string[] args =
        {
            "-c", "qux",
            "-p", "AuthServer",
            "-e", "5000",
            "-n", "testdbname"
        };

        Sut.ReplaceOptionsHandler();

        // Action:
        await Sut.RunAsync(args);

        // Assert:
        await Assert.OptionsHandlerReceivedContainerVerb();
    }
}

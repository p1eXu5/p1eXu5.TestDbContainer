using p1eXu5.AspNetCore.Testing;
using p1eXu5.CliBootstrap.CommandLineParser;
using p1eXu5.TestDbContainer.Options;
using p1eXu5.TestDbContainer.Tests.Unit.Handlers;

namespace p1eXu5.TestDbContainer.Tests.Integration;

public abstract class TestsBase
{
    [SetUp]
    public void Initialize()
    {
        Sut = new Sut();
        Assert = new AssertMocksBase<TestsBase>(this);
    }

    internal protected Sut Sut { get; private set; }

    protected AssertMocksBase<TestsBase> Assert { get; set; }

    // --------------------- mock helper classes

    protected class AssertMocksBase<TTests> where TTests : TestsBase
    {
        private protected readonly TTests _;

        internal AssertMocksBase(TTests testsBase)
        {
            _ = testsBase;
        }

        internal async Task OptionsHandlerReceivedContainerVerb()
        {
            _.Sut.OptionsHandler.Should().NotBeNull();
            await _.Sut.OptionsHandler!.Received(1)
                .HandleAsync(
                    Verify.That<SuccessParsingResult>(res =>
                    {
                        res.Should().NotBeNull();
                        res.Should().BeOfType<SuccessParsingResult.Success<ContainerVerb>>();
                    })!,
                    Arg.Any<CancellationToken>());
        }
    }
}

namespace p1eXu5.TestDbContainer.Tests.Integration;

public abstract class TestsBase
{
    public TestsBase()
    {
        Sut = new Sut();
    }

    internal protected Sut Sut { get; }
}


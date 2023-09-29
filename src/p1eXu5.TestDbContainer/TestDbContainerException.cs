namespace p1eXu5.TestDbContainer;

public class TestDbContainerException : InvalidOperationException
{
    public TestDbContainerException()
    {
    }

    public TestDbContainerException(string message) : base(message)
    {
    }

    public TestDbContainerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

namespace p1eXu5.TestDbContainer.Exceptions;

public sealed class ComposeFileNotFoundException : FileNotFoundException
{
    public ComposeFileNotFoundException(string message) : base(message)
    {
    }

    public ComposeFileNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ComposeFileNotFoundException()
    {
    }
}

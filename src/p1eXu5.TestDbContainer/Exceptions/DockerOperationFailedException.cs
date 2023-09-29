namespace p1eXu5.TestDbContainer.Exceptions;

public class DockerOperationFailedException : InvalidOperationException
{
    public DockerOperationFailedException()
    {
    }

    public DockerOperationFailedException(string message) : base(message)
    {
    }

    public DockerOperationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

namespace p1eXu5.TestDbContainer.Exceptions;

public sealed class ServiceNotFoundException : ArgumentException
{
    public ServiceNotFoundException(string message) : base(message)
    {
    }

    public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ServiceNotFoundException()
    {
    }
}

namespace DashiToon.Api.Domain.Exceptions;

public class PublishMoreThanOnceException : Exception
{
    public PublishMoreThanOnceException() : base("Chapter is already published.")
    {
    }
}

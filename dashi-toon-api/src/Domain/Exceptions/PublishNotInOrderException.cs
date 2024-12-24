namespace DashiToon.Api.Domain.Exceptions;

public class PublishNotInOrderException : Exception
{
    public PublishNotInOrderException() : base($"Previous chapter is not published.")
    {
    }
}

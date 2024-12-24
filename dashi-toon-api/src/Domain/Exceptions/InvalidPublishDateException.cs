namespace DashiToon.Api.Domain.Exceptions;

public class InvalidPublishDateException : Exception
{
    public InvalidPublishDateException() : base("Publish date must be in the future.")
    {
    }
}

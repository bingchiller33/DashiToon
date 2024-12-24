namespace DashiToon.Api.Domain.Exceptions;

public class AlreadyCheckinException : Exception
{
    public AlreadyCheckinException() : base("User is already checked-in")
    {
    }
}

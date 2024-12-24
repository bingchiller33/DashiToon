namespace DashiToon.Api.Domain.Exceptions;

public class AlreadySubscribedException : Exception
{
    public AlreadySubscribedException() : base("User is already subscribing to this series")
    {
    }
}

namespace DashiToon.Api.Domain.Exceptions;

public class AlreadyFollowedException : Exception
{
    public AlreadyFollowedException() : base("User has already followed this series.")
    {
    }
}

namespace DashiToon.Api.Domain.Exceptions;

public class UpdateDashiFanCoolDownException : Exception
{
    public UpdateDashiFanCoolDownException() : base("Cannot update dashi fan more than once in a month.")
    {
    }
}

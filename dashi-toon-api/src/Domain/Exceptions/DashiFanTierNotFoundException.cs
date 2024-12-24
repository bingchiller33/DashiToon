namespace DashiToon.Api.Domain.Exceptions;

public class DashiFanTierNotFoundException : Exception
{
    public DashiFanTierNotFoundException() : base("Dashi fan tier not found")
    {
    }
}

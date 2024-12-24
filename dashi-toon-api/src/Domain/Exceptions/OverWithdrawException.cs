namespace DashiToon.Api.Domain.Exceptions;

public class OverWithdrawException : Exception
{
    public OverWithdrawException() : base("Users doesn't have enough fund to withdraw.")
    {
    }
}

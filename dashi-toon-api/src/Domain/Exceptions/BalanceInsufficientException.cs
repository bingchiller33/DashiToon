namespace DashiToon.Api.Domain.Exceptions;

public class BalanceInsufficientException : Exception
{
    public BalanceInsufficientException() : base(
        "User doesn't have enough kana coin or kana gold to unlock the chapter")
    {
    }
}

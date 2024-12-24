namespace DashiToon.Api.Domain.Exceptions;

public class ChapterAlreadyUnlockedException : Exception
{
    public ChapterAlreadyUnlockedException() : base("Chapter is already unlocked")
    {
    }
}

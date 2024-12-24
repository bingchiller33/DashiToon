namespace DashiToon.Api.Domain.Exceptions;

public class NotPublishedChapterCannotBeUnlockedException : Exception
{
    public NotPublishedChapterCannotBeUnlockedException() : base("Unpublished chapter cannot be unlocked")
    {
    }
}

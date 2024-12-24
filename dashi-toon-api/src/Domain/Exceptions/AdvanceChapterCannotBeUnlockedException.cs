namespace DashiToon.Api.Domain.Exceptions;

public class AdvanceChapterCannotBeUnlockedException : Exception
{
    public AdvanceChapterCannotBeUnlockedException() : base("Cannot unlock advance chapter")
    {
    }
}

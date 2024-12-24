namespace DashiToon.Api.Domain.Exceptions;

public class UnpublishNonPublishedChapterException : Exception
{
    public UnpublishNonPublishedChapterException() : base(
        "Chapter is not published.")
    {
    }
}

namespace DashiToon.Api.Domain.Exceptions;

public class ChapterVersionNotFoundException : Exception
{
    public ChapterVersionNotFoundException() : base("Chapter version not found")
    {
    }
}

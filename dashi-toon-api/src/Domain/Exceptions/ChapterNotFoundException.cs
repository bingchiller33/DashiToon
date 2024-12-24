namespace DashiToon.Api.Domain.Exceptions;

public class ChapterNotFoundException : Exception
{
    public ChapterNotFoundException() : base("Chapter not found")
    {
    }
}

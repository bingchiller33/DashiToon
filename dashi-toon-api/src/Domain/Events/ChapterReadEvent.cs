namespace DashiToon.Api.Domain.Events;

public class ChapterReadEvent : BaseEvent
{
    public ChapterReadEvent(int chapterId)
    {
        ChapterId = chapterId;
    }

    public int ChapterId { get; }
}

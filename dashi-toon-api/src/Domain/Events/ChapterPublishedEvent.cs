namespace DashiToon.Api.Domain.Events;

public class ChapterPublishedEvent : BaseEvent
{
    public ChapterPublishedEvent(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}

namespace DashiToon.Api.Domain.Events;

public class ChapterScheduledEvent : BaseEvent
{
    public ChapterScheduledEvent(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}

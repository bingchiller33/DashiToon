namespace DashiToon.Api.Domain.Events;

public class SeriesCreatedEvent : BaseEvent
{
    public SeriesCreatedEvent(Series series)
    {
        Series = series;
    }

    public Series Series { get; }
}

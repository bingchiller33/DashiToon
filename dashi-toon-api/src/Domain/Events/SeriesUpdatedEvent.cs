namespace DashiToon.Api.Domain.Events;

public class SeriesUpdatedEvent : BaseEvent
{
    public SeriesUpdatedEvent(Series series)
    {
        Series = series;
    }

    public Series Series { get; }
}

namespace DashiToon.Api.Domain.Events;

public class DashiFanStatusUpdatedEvent : BaseEvent
{
    public DashiFanStatusUpdatedEvent(DashiFan tier)
    {
        Tier = tier;
    }

    public DashiFan Tier { get; }
}

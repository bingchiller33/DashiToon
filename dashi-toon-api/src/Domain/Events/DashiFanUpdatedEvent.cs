namespace DashiToon.Api.Domain.Events;

public class DashiFanUpdatedEvent : BaseEvent
{
    public DashiFanUpdatedEvent(DashiFan tier)
    {
        Tier = tier;
    }

    public DashiFan Tier { get; }
}

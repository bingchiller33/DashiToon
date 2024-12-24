namespace DashiToon.Api.Domain.Events;

public class DashiFanCreatedEvent : BaseEvent
{
    public DashiFanCreatedEvent(DashiFan tier)
    {
        Tier = tier;
    }

    public DashiFan Tier { get; }
}

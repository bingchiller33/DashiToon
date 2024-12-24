namespace DashiToon.Api.Domain.Events;

public class DashiFanPriceUpdatedEvent : BaseEvent
{
    public DashiFanPriceUpdatedEvent(DashiFan tier)
    {
        Tier = tier;
    }

    public DashiFan Tier { get; }
}

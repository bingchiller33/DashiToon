namespace DashiToon.Api.Domain.Events;

public class UserMutedEvent : BaseEvent
{
    public UserMutedEvent(string userId, DateTimeOffset mutedUntil)
    {
        UserId = userId;
        MutedUntil = mutedUntil;
    }

    public string UserId { get; }
    public DateTimeOffset MutedUntil { get; set; }
}

namespace DashiToon.Api.Domain.Events;

public class AuthorRestrictedEvent : BaseEvent
{
    public AuthorRestrictedEvent(string userId, DateTimeOffset restrictedUntil)
    {
        UserId = userId;
        RestrictedUntil = restrictedUntil;
    }

    public string UserId { get; }
    public DateTimeOffset RestrictedUntil { get; }
}

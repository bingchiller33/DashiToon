namespace DashiToon.Api.Domain.Events;

public class UserReviewedEvent : BaseEvent
{
    public UserReviewedEvent(Review review)
    {
        Review = review;
    }

    public Review Review { get; }
}

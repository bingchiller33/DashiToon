namespace DashiToon.Api.Domain.Events;

public class UserCommentedEvent : BaseEvent
{
    public UserCommentedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}

namespace DashiToon.Api.Domain.Entities;

public class Comment : BaseAuditableEntity<Guid>
{
    private readonly List<Comment> _replies = [];
    private readonly List<CommentRate> _commentRates = [];

    private Comment()
    {
    }

    public int ChapterId { get; private set; }
    public string UserId { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public int Likes { get; private set; }
    public int Dislikes { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }
    public IReadOnlyList<Comment> Replies => _replies.AsReadOnly();
    public IReadOnlyList<CommentRate> CommentRates => _commentRates.AsReadOnly();

    public static Comment Create(
        string userId,
        int chapterId,
        string content)
    {
        ValidateContent(content);

        Comment? comment = new()
        {
            ChapterId = chapterId, UserId = userId, Content = content, Timestamp = DateTimeOffset.UtcNow
        };

        comment.AddDomainEvent(new UserCommentedEvent(comment));

        return comment;
    }

    public void Update(string content)
    {
        ValidateContent(content);

        Content = content;
        Timestamp = DateTimeOffset.UtcNow;

        AddDomainEvent(new UserCommentedEvent(this));
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrEmpty(content) || content.Length is < 1 or > 2048)
        {
            throw new ArgumentException("Comment content can only have a length between 100 and 2048 characters",
                nameof(content));
        }
    }

    public void Rate(string byUserId, bool isLiked)
    {
        CommentRate? rate = _commentRates.SingleOrDefault(r => r.UserId == byUserId);

        if (rate is null)
        {
            rate = CommentRate.Create(byUserId, isLiked);

            _commentRates.Add(rate);
        }

        rate.IsLiked = isLiked;

        Likes = _commentRates.Count(r => r.IsLiked);
        Dislikes = _commentRates.Count(r => !r.IsLiked);
    }

    public void ReplyComment(
        string userId,
        string content)
    {
        _replies.Add(Create(
            userId,
            ChapterId,
            content
        ));
    }
}

public class CommentRate
{
    public string UserId { get; private init; } = null!;
    public bool IsLiked { get; internal set; }

    private CommentRate()
    {
    }

    internal static CommentRate Create(string userId, bool isLiked)
    {
        return new CommentRate { UserId = userId, IsLiked = isLiked };
    }
}

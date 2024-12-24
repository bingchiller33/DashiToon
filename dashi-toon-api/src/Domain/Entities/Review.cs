namespace DashiToon.Api.Domain.Entities;

public class Review : BaseAuditableEntity<Guid>
{
    private readonly List<ReviewRate> _reviewRates = [];

    private Review()
    {
    }

    public int SeriesId { get; private set; }
    public string UserId { get; private set; } = null!;
    public bool IsRecommended { get; private set; }
    public string Content { get; private set; } = null!;
    public int Likes { get; private set; }
    public int Dislikes { get; private set; }
    public DateTimeOffset Timestamp { get; set; }
    public IReadOnlyList<ReviewRate> ReviewRates => _reviewRates.AsReadOnly();
    public Series Series { get; private set; } = null!;

    public static Review Create(
        string userId,
        int seriesId,
        bool isRecommended,
        string content)
    {
        ValidateContent(content);

        Review? review = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SeriesId = seriesId,
            Content = content,
            IsRecommended = isRecommended,
            Timestamp = DateTimeOffset.UtcNow
        };

        review.AddDomainEvent(new UserReviewedEvent(review));

        return review;
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrEmpty(content) || content.Length is < 100 or > 8192)
        {
            throw new ArgumentException("Review content can only have a length between 100 and 8192 characters",
                nameof(content));
        }
    }

    public void Rate(string byUserId, bool isLiked)
    {
        ReviewRate? rate = _reviewRates.SingleOrDefault(r => r.UserId == byUserId);

        if (rate is null)
        {
            rate = ReviewRate.Create(byUserId, isLiked);

            _reviewRates.Add(rate);
        }

        rate.IsLiked = isLiked;

        Likes = _reviewRates.Count(r => r.IsLiked);
        Dislikes = _reviewRates.Count(r => !r.IsLiked);
    }

    public void Update(
        string content,
        bool isRecommended)
    {
        ValidateContent(content);

        IsRecommended = isRecommended;
        Content = content;
        Timestamp = DateTimeOffset.UtcNow;

        AddDomainEvent(new UserReviewedEvent(this));
    }
}

public class ReviewRate
{
    public string UserId { get; private init; } = null!;
    public bool IsLiked { get; internal set; }

    private ReviewRate()
    {
    }

    internal static ReviewRate Create(string userId, bool isLiked)
    {
        return new ReviewRate { UserId = userId, IsLiked = isLiked };
    }
}

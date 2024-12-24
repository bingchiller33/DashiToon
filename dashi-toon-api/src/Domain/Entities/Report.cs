namespace DashiToon.Api.Domain.Entities;

public class Report : BaseAuditableEntity<Guid>
{
    public string? Reported { get; private set; }
    public ReportType Type { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset ReportedAt { get; private set; }
    public ReportStatus ReportStatus { get; private set; }
    public Guid? CommentId { get; private set; }
    public Guid? ReviewId { get; private set; }
    public int? SeriesId { get; private set; }
    public int? ChapterId { get; private set; }

    public Comment? Comment { get; private set; }
    public Review? Review { get; private set; }
    public Series? Series { get; private set; }
    public Chapter? Chapter { get; private set; }
    public ModerationAnalysis? Analysis { get; private set; }

    protected Report()
    {
    }

    public static Report CreateNewUserReport(string reported, ReportType type, string reason, object entityId)
    {
        Report report = new()
        {
            Reported = reported,
            Reason = reason,
            ReportedAt = DateTimeOffset.UtcNow,
            ReportStatus = ReportStatus.Pending
        };

        report.OfType(type, entityId);

        return report;
    }


    public static Report CreateNewSystemReport(ReportType type, object reportedEntityId)
    {
        Report report = new()
        {
            Reason = "Hệ thống kiểm duyệt tự động",
            ReportedAt = DateTimeOffset.UtcNow,
            ReportStatus = ReportStatus.Pending
        };

        report.OfType(type, reportedEntityId);

        return report;
    }

    private void OfType(ReportType type, object entityId)
    {
        Type = type;

        switch (type)
        {
            case ReportType.Review:
                if (entityId is not Guid reviewId)
                {
                    throw new ArgumentException("EntityId is not a Guid");
                }

                ReviewId = reviewId;
                break;
            case ReportType.Comment:
                if (entityId is not Guid commentId)
                {
                    throw new ArgumentException("EntityId is not a Guid");
                }

                CommentId = commentId;
                break;
            case ReportType.Content:
                if (entityId is not int chapterId)
                {
                    throw new ArgumentException("EntityId is not a int");
                }

                ChapterId = chapterId;
                break;
            case ReportType.Series:
                if (entityId is not int seriesId)
                {
                    throw new ArgumentException("EntityId is not a int");
                }

                SeriesId = seriesId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void AddAnalytics(ModerationAnalysis analysis)
    {
        Analysis = ModerationAnalysis.Create(analysis.Flagged, analysis.FlaggedCategories);
    }

    public void DismissReport()
    {
        ReportStatus = ReportStatus.Dismissed;
    }

    public void ResolveReport()
    {
        ReportStatus = ReportStatus.ActionTaken;
    }
}

public enum ReportType
{
    Review,
    Comment,
    Content,
    Series
}

public enum ReportStatus
{
    Pending,
    ActionTaken,
    Dismissed
}

public class ModerationAnalysis
{
    public bool Flagged { get; private set; }
    public List<CategoryScore> FlaggedCategories { get; private set; } = [];
    public DateTimeOffset AnalyzedAt { get; private set; }

    private ModerationAnalysis()
    {
    }

    public static ModerationAnalysis Create(
        bool flagged,
        List<CategoryScore> flaggedCategories,
        DateTimeOffset? analyzedAt = null)
    {
        return new ModerationAnalysis
        {
            Flagged = flagged,
            FlaggedCategories = flaggedCategories,
            AnalyzedAt = analyzedAt ?? DateTimeOffset.UtcNow
        };
    }
}

public class CategoryScore
{
    public string Category { get; init; } = null!;
    public float Score { get; init; }
}

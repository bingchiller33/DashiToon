using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class ReportTests
{
    [Fact]
    public void CreateCommentUserReportShouldCreateSuccessfully()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        Guid commentId = Guid.NewGuid();

        // Act
        Report? report = Report.CreateNewUserReport(reportUser.Id, ReportType.Comment, reason, commentId);

        // Assert
        report.Reported.Should().Be(reportUser.Id);
        report.Type.Should().Be(ReportType.Comment);
        report.Reason.Should().Be(reason);
        report.ReportedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        report.ReportStatus.Should().Be(ReportStatus.Pending);
        report.CommentId.Should().Be(commentId);
    }

    [Fact]
    public void CreateCommentUserReportShouldRequireValidEntityId()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        int commentId = 1;

        // Act
        FluentActions.Invoking(() => Report.CreateNewUserReport(reportUser.Id, ReportType.Comment, reason, commentId))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateReviewUserReportShouldCreateSuccessfully()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        Guid reviewId = Guid.NewGuid();

        // Act
        Report? report = Report.CreateNewUserReport(reportUser.Id, ReportType.Review, reason, reviewId);

        // Assert
        report.Reported.Should().Be(reportUser.Id);
        report.Type.Should().Be(ReportType.Review);
        report.Reason.Should().Be(reason);
        report.ReportedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        report.ReportStatus.Should().Be(ReportStatus.Pending);
        report.ReviewId.Should().Be(reviewId);
    }

    [Fact]
    public void CreateReviewUserReportShouldRequireValidEntityId()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        int reviewId = 1;

        // Act
        FluentActions.Invoking(() => Report.CreateNewUserReport(reportUser.Id, ReportType.Review, reason, reviewId))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateContentUserReportShouldCreateSuccessfully()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        int chapterId = 1;

        // Act
        Report? report = Report.CreateNewUserReport(reportUser.Id, ReportType.Content, reason, chapterId);

        // Assert
        report.Reported.Should().Be(reportUser.Id);
        report.Type.Should().Be(ReportType.Content);
        report.Reason.Should().Be(reason);
        report.ReportedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        report.ReportStatus.Should().Be(ReportStatus.Pending);
        report.ChapterId.Should().Be(chapterId);
    }

    [Fact]
    public void CreateContentUserReportShouldRequireValidEntityId()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        Guid chapterId = Guid.NewGuid();

        // Act
        FluentActions.Invoking(() => Report.CreateNewUserReport(reportUser.Id, ReportType.Content, reason, chapterId))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSeriesUserReportShouldCreateSuccessfully()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        int seriesId = 1;

        // Act
        Report? report = Report.CreateNewUserReport(reportUser.Id, ReportType.Series, reason, seriesId);

        // Assert
        report.Reported.Should().Be(reportUser.Id);
        report.Type.Should().Be(ReportType.Series);
        report.Reason.Should().Be(reason);
        report.ReportedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        report.ReportStatus.Should().Be(ReportStatus.Pending);
        report.SeriesId.Should().Be(seriesId);
    }

    [Fact]
    public void CreateSeriesUserReportShouldRequireValidEntityId()
    {
        // Arrange
        ApplicationUser? reportUser = new();
        string? reason = "Test Reason";
        Guid seriesId = Guid.NewGuid();

        // Act
        FluentActions.Invoking(() => Report.CreateNewUserReport(reportUser.Id, ReportType.Series, reason, seriesId))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSystemReportShouldCreateSuccessfully()
    {
        // Arrange
        ReportType type = ReportType.Series;
        int seriesId = 1;

        // Act
        Report? report = Report.CreateNewSystemReport(type, seriesId);

        // Assert
        report.Reported.Should().BeNull();
        report.Type.Should().Be(ReportType.Series);
        report.Reason.Should().Be("Hệ thống kiểm duyệt tự động");
        report.ReportedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        report.ReportStatus.Should().Be(ReportStatus.Pending);
        report.SeriesId.Should().Be(seriesId);
    }

    [Fact]
    public void AddModerationAnalysisShouldSuccess()
    {
        // Arrange
        ReportType type = ReportType.Series;
        int seriesId = 1;

        Report? report = Report.CreateNewSystemReport(type, seriesId);

        // Act
        report.AddAnalytics(ModerationAnalysis.Create(
            true,
            [new CategoryScore { Category = "sexual", Score = 0.9f }]));

        // Assert
        report.Analysis.Should().NotBeNull();
        report.Analysis!.Flagged.Should().BeTrue();
        report.Analysis!.FlaggedCategories.Should()
            .BeEquivalentTo([new CategoryScore { Category = "sexual", Score = 0.9f }]);
        report.Analysis!.AnalyzedAt.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void DismissReportShouldChangeStatus()
    {
        // Arrange
        ReportType type = ReportType.Series;
        int seriesId = 1;

        Report? report = Report.CreateNewSystemReport(type, seriesId);

        // Act
        report.DismissReport();

        // Assert
        report.ReportStatus.Should().Be(ReportStatus.Dismissed);
    }

    [Fact]
    public void ResolveReportShouldChangeStatus()
    {
        // Arrange
        ReportType type = ReportType.Series;
        int seriesId = 1;

        Report? report = Report.CreateNewSystemReport(type, seriesId);

        // Act
        report.ResolveReport();

        // Assert
        report.ReportStatus.Should().Be(ReportStatus.ActionTaken);
    }
}

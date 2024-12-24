using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.Moderation.Commands.ReportSeries;
using DashiToon.Api.Application.Moderation.Commands.ResolveSeriesReport;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Application.Moderation.Queries.IsAuthorAllowedToPublish;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ResolveSeriesReportTest : BaseIntegrationTest
{
    public ResolveSeriesReportTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task ResolveSeriesReportShouldRestrictPublishUser()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            ["Auth 1"],
            null));

        await SendAsync(new ReportSeriesCommand(seriesId, "Test Reason"));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new ResolveSeriesReportCommand(seriesId, 7));

        // Assert
        await RunAsDefaultUserAsync();
        AllowVm? result = await SendAsync(new IsAuthorAllowedToPublishQuery());

        result.IsAllowed.Should().BeFalse();
        DateTimeOffset.Parse(result.NotAllowedUntil!).Should()
            .BeCloseTo(DateTimeOffset.UtcNow.AddDays(7), TimeSpan.FromMilliseconds(100));
    }
}

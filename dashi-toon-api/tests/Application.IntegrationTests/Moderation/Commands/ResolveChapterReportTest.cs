using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Moderation.Commands.ResolveChapterReport;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Application.Moderation.Queries.IsAuthorAllowedToPublish;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Moderation.Commands;

using static Testing;

[Collection("Serialize")]
public class ResolveChapterReportTest : BaseIntegrationTest
{
    public ResolveChapterReportTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task ResolveChapterReportShouldRestrictPublishUser()
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

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        await SendAsync(new PublishChapterCommand(
            seriesId,
            volumeId,
            chapterId));

        await RunAsModeratorAsync();
        // Act
        await SendAsync(new ResolveChapterReportCommand(chapterId, 7));

        // Assert
        await RunAsDefaultUserAsync();
        AllowVm? result = await SendAsync(new IsAuthorAllowedToPublishQuery());

        result.IsAllowed.Should().BeFalse();
        DateTimeOffset.Parse(result.NotAllowedUntil!).Should()
            .BeCloseTo(DateTimeOffset.UtcNow.AddDays(7), TimeSpan.FromMilliseconds(100));
    }
}

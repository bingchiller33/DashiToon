using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Models;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.Users.Commands.CompleteMission;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Application.Users.Queries.GetKanaTotals;
using DashiToon.Api.Application.Users.Queries.GetKanaTransactions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Commands;

using static Testing;

[Collection("Serialize")]
public class CompleteMissionTest : BaseIntegrationTest
{
    public CompleteMissionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CompleteMissionShouldTopUpKanaCoin()
    {
        // Arrange
        await RunAsAdministratorAsync();

        MissionVm? mission1 = await SendAsync(new CreateMissionCommand(1, 50));
        MissionVm? mission2 = await SendAsync(new CreateMissionCommand(2, 50));

        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new FollowSeriesCommand(seriesId));

        await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Act
        await SendAsync(new CompleteMissionCommand(mission1.Id));

        // Assert
        KanaTotalsVm? kanaTotals = await SendAsync(new GetKanaTotalsQuery());
        PaginatedList<KanaTransactionVm>? kanaTransactions = await SendAsync(new GetKanaTransactionsQuery("EARN"));

        kanaTotals.Totals.First().KanaType.Should().Be(KanaType.Coin);
        kanaTotals.Totals.First().Amount.Should().Be(50);

        kanaTransactions.Items.First().Type.Should().Be(TransactionType.Mission);
        kanaTransactions.Items.First().Currency.Should().Be(KanaType.Coin);
        kanaTransactions.Items.First().Amount.Should().Be(50);
    }

    [Fact]
    public async Task CompleteMissionShouldRequireInactiveMission()
    {
        // Arrange
        await RunAsAdministratorAsync();

        MissionVm? mission1 = await SendAsync(new CreateMissionCommand(1, 50, false));
        MissionVm? mission2 = await SendAsync(new CreateMissionCommand(2, 50));

        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new FollowSeriesCommand(seriesId));

        await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        // Act
        await FluentActions.Invoking(() => SendAsync(new CompleteMissionCommand(mission1.Id)))
            .Should().ThrowAsync<NotFoundException>();
    }
}

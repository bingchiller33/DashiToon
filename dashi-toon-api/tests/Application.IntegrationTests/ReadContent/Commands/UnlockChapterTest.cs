using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.SetChapterPricing;
using DashiToon.Api.Application.AuthorStudio.Revenue.Models;
using DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenue;
using DashiToon.Api.Application.AuthorStudio.Revenue.Queries.GetRevenueTransactions;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.ReadContent.Commands.UnlockChapter;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Queries.GetKanaTotals;
using DashiToon.Api.Application.Users.Queries.GetKanaTransactions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.ReadContent.Commands;

using static Testing;

[Collection("Serialize")]
public class UnlockChapterTest : BaseIntegrationTest
{
    public UnlockChapterTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UnlockChapterShouldSucceed()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        await SendAsync(new SetChapterPricingCommand(seriesId, volumeId, chapterId, 30));
        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new CheckinUserCommand());

        // Act
        await SendAsync(new UnlockChapterCommand(seriesId, volumeId, chapterId));

        // Assert
        KanaTotalsVm? kanaTotals = await SendAsync(new GetKanaTotalsQuery());
        kanaTotals.Totals[0].KanaType.Should().Be(KanaType.Coin);
        kanaTotals.Totals[0].Amount.Should().Be(70);

        PaginatedList<KanaTransactionVm>? spentTransactions = await SendAsync(new GetKanaTransactionsQuery("SPENT"));
        spentTransactions.Items.First().Amount.Should().Be(-30);
        spentTransactions.Items.First().Currency.Should().Be(KanaType.Coin);

        RevenueVm? revenue = await SendAsync(new GetRevenueQuery());
        revenue.TotalRevenue.Should().Be(210);

        PaginatedList<RevenueTransactionVm>? revenueTransactions =
            await SendAsync(new GetRevenueTransactionsQuery(RevenueTransactionType.Earn));
        revenueTransactions.Items.First().Amount.Should().Be(210);
    }

    [Fact]
    public async Task UnlockChapterShouldRequirePublishedSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UnlockChapterCommand(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UnlockChapterShouldRequireExistingVolume()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UnlockChapterCommand(seriesId, 1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UnlockChapterShouldRequirePublishedChapter()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
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

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            """<img src="" data-img-name="bruh.png"/>""",
            "TestNote1"
        ));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UnlockChapterCommand(seriesId, volumeId, chapterId)))
            .Should().ThrowAsync<NotFoundException>();
    }
}

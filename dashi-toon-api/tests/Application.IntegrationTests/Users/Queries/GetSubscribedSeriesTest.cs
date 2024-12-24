using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSubscribedSeriesTest : BaseIntegrationTest
{
    public GetSubscribedSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSubscribedSeriesShouldReturnAllSubscriptions()
    {
        // Arrange
        string? userId = await RunAsDefaultUserAsync();

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
            ["Auth 1"],
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
            ["Auth 1"],
            null));

        Guid tierId = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            10_000,
            "VND"));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tierId, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tierId && s.Status == SubscriptionStatus.Pending);
        subscription.Activate(DateTimeOffset.UtcNow.AddDays(30));

        await context.SaveChangesAsync(default);

        // Act
        List<UserSubscriptionVm>? subscribedSeries = await SendAsync(new GetSubscriptionsQuery());

        // Assert
        subscribedSeries.Should().HaveCount(1);
        subscribedSeries[0].SubscriptionId.Should().Be(subscription.Id);
        subscribedSeries[0].Series.Id.Should().Be(seriesId);
        subscribedSeries[0].Series.ThumbnailUrl.Should().Be("thumbnails/TestThumbnail");
        subscribedSeries[0].Series.Title.Should().Be("TestTitle");
        subscribedSeries[0].Series.Author.Should().Be("Auth 1");
        subscribedSeries[0].DashiFan.Id.Should().Be(tierId);
        subscribedSeries[0].DashiFan.Name.Should().Be("TestTier");
        subscribedSeries[0].DashiFan.Description.Should().Be("TestDescription");
        subscribedSeries[0].DashiFan.Perks.Should().Be(4);
        subscribedSeries[0].DashiFan.Price!.Amount.Should().Be(10_000);
        subscribedSeries[0].DashiFan.Price!.Currency.Should().Be("VND");
        subscribedSeries[0].NextBillingDate.Should().NotBeEmpty();
        subscribedSeries[0].SubscribedSince.Should().NotBeEmpty();
        subscribedSeries[0].Status.Should().Be(SubscriptionStatus.Active);
    }
}

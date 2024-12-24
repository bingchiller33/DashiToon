using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Application.Users.Queries.GetSeriesSubscription;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesSubscriptionTest : BaseIntegrationTest
{
    public GetSeriesSubscriptionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesSubscriptionShouldReturnSubscriptionDetail()
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
        SeriesSubscriptionVm? subscriptionDetail = await SendAsync(new GetSeriesSubscriptionQuery(seriesId));

        // Assert
        subscriptionDetail.Should().NotBeNull();
        subscriptionDetail!.SubscriptionId.Should().Be(subscription.Id);
        subscriptionDetail.Tier.Id.Should().Be(tierId);
        subscriptionDetail.Tier.Name.Should().Be("TestTier");
        subscriptionDetail.Tier.Description.Should().Be("TestDescription");
        subscriptionDetail.Tier.Perks.Should().Be(4);
        subscriptionDetail.Tier.Price!.Amount.Should().Be(10_000);
        subscriptionDetail.Tier.Price!.Currency.Should().Be("VND");
        subscriptionDetail.Status.Should().Be(SubscriptionStatus.Active);
        subscriptionDetail.NextBillingDate.Should().NotBeEmpty();
    }
}

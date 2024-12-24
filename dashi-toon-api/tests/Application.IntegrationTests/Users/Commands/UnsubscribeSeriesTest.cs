using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Application.Users.Commands.UnsubscribeSeries;
using DashiToon.Api.Application.Users.Queries.GetSeriesSubscription;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Users.Commands;

using static Testing;

[Collection("Serialize")]
public class UnsubscribeSeriesTest : BaseIntegrationTest
{
    public UnsubscribeSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UnsubscribeSeriesShouldCancelSubscription()
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
        await SendAsync(new UnsubscribeSeriesCommand(subscription.Id));

        // Assert
        SeriesSubscriptionVm? updatedSubscription = await SendAsync(new GetSeriesSubscriptionQuery(seriesId));

        updatedSubscription.Should().NotBeNull();
        updatedSubscription!.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task UnsubscribeSeriesShouldRequireActiveSubscription()
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

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UnsubscribeSeriesCommand(subscription.Id)))
            .Should().ThrowAsync<NotFoundException>();
    }
}

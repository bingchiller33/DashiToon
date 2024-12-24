using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.CreateDashiFan;
using DashiToon.Api.Application.AuthorStudio.DashiFans.Commands.UpdateDashiFanStatus;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Users.Commands.DowngradeSubscriptionTier;
using DashiToon.Api.Application.Users.Commands.SubscribeSeries;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Users.Commands;

using static Testing;

[Collection("Serialize")]
public class DowngradeSubscriptionTierTest : BaseIntegrationTest
{
    public DowngradeSubscriptionTierTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task DowngradeSubscriptionTierShouldUpdateTier()
    {
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

        Guid tier1Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            10_000,
            "VND"));

        Guid tier2Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            5,
            20_000,
            "VND"));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tier2Id, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tier2Id && s.Status == SubscriptionStatus.Pending);
        subscription.Activate(DateTimeOffset.UtcNow.AddDays(30));

        await context.SaveChangesAsync(default);

        // Act
        await SendAsync(new DowngradeSubscriptionTierCommand(subscription.Id, tier1Id));

        // Assert
        Subscription? downgradedSubscription = await context.Subscriptions.FirstOrDefaultAsync(s =>
            s.DashiFanId == tier1Id && s.Status == SubscriptionStatus.Active);

        downgradedSubscription.Should().NotBeNull();
    }

    [Fact]
    public async Task DowngradeSubscriptionTierShouldRequireExistingSubscription()
    {
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

        // Act & Assert
        await FluentActions
            .Invoking(() => SendAsync(new DowngradeSubscriptionTierCommand(Guid.NewGuid(), Guid.NewGuid())))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task DowngradeSubscriptionTierShouldOnlyAllowSubscribedUser()
    {
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

        Guid tier1Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            10_000,
            "VND"));

        Guid tier2Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            5,
            20_000,
            "VND"));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tier2Id, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tier2Id && s.Status == SubscriptionStatus.Pending);

        await RunAsUserAsync("User1", "Bruh@1311", []);

        // Act & Assert
        await FluentActions
            .Invoking(() => SendAsync(new DowngradeSubscriptionTierCommand(subscription.Id, tier1Id)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task DowngradeSubscriptionTierShouldRequireActiveTier()
    {
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

        Guid tier1Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            10_000,
            "VND"));

        await SendAsync(new UpdateDashiFanStatusCommand(seriesId, tier1Id));

        Guid tier2Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            5,
            20_000,
            "VND"));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tier2Id, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tier2Id && s.Status == SubscriptionStatus.Pending);

        // Act & Assert
        await FluentActions
            .Invoking(() => SendAsync(new DowngradeSubscriptionTierCommand(subscription.Id, tier1Id)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DowngradeSubscriptionTierShouldRequireActiveSubscription()
    {
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

        Guid tier1Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            4,
            10_000,
            "VND"));

        Guid tier2Id = await SendAsync(new CreateDashiFanCommand(
            seriesId,
            "TestTier",
            "TestDescription",
            5,
            20_000,
            "VND"));

        await SendAsync(new SubscribeSeriesCommand(seriesId, tier2Id, string.Empty, string.Empty));

        IApplicationDbContext? context = GetContext();
        Subscription? subscription = await context.Subscriptions.FirstAsync(s =>
            s.DashiFanId == tier2Id && s.Status == SubscriptionStatus.Pending);

        // Act & Assert
        await FluentActions
            .Invoking(() => SendAsync(new DowngradeSubscriptionTierCommand(subscription.Id, tier1Id)))
            .Should().ThrowAsync<ChangeSubscriptionTierException>();
    }
}

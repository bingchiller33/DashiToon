namespace DashiToon.Api.Domain.Services;

public class ChapterService
{
    private static readonly object Sync = new();

    public void UnlockChapter(IDomainUser user, Chapter chapter)
    {
        lock (Sync)
        {
            if (chapter.PublishedVersionId is null)
            {
                throw new NotPublishedChapterCannotBeUnlockedException();
            }

            if (chapter.IsAdvanceChapter)
            {
                throw new AdvanceChapterCannotBeUnlockedException();
            }

            if (user.UnlockedChapters.Contains(chapter))
            {
                throw new ChapterAlreadyUnlockedException();
            }

            if (user.KanaCoin >= chapter.KanaPrice)
            {
                user.AddTransaction(KanaTransaction.Create(
                    KanaType.Coin,
                    TransactionType.Spend,
                    (chapter.KanaPrice ?? 0) * -1,
                    $"Mua chương truyện [{chapter.Id}]",
                    chapter.Id
                ));

                user.UnlockChapter(chapter);
            }
            else if (user.KanaGold >= chapter.KanaPrice)
            {
                user.AddTransaction(KanaTransaction.Create(
                    KanaType.Gold,
                    TransactionType.Spend,
                    (chapter.KanaPrice ?? 0) * -1,
                    $"Mua chương truyện [{chapter.Id}]",
                    chapter.Id
                ));

                user.UnlockChapter(chapter);
            }
            else
            {
                throw new BalanceInsufficientException();
            }
        }
    }

    public bool IsGuestUserAllowedToReadChapter(List<Chapter> seriesChapter, int chapterId)
    {
        Chapter? chapter = seriesChapter.FirstOrDefault(c => c.Id == chapterId);

        return chapter switch
        {
            null => throw new ChapterNotFoundException(),
            { IsAdvanceChapter: false, KanaPrice: null or 0 } => true,
            _ => false
        };
    }

    public bool IsUserAllowedToReadChapter(IDomainUser user, int seriesId, List<Chapter> seriesChapter, int chapterId)
    {
        Chapter? chapter = seriesChapter.First(c => c.Id == chapterId);

        bool isSubscribed = SubscriptionService.IsUserSubscribedToSeries(user, seriesId);

        if (!chapter.IsAdvanceChapter)
        {
            return isSubscribed || user.UnlockedChapters.Select(c => c.Id).Contains(chapterId);
        }

        if (isSubscribed)
        {
            return false;
        }

        Subscription? subscription = user.Subscriptions
            .Where(s =>
                s.Tier.SeriesId == seriesId
                && (s.Status is SubscriptionStatus.Active
                    || (s.Status is SubscriptionStatus.Cancelled && s.NextBillingDate > DateTimeOffset.UtcNow)))
            .OrderByDescending(s => s.Tier.Perks)
            .First();

        int allowedAdvanceChapterCount = subscription.Tier.Perks;

        List<Chapter>? allowedAdvancedChapters = seriesChapter
            .Where(c => c.IsAdvanceChapter)
            .OrderBy(c => c.Volume.VolumeNumber)
            .ThenBy(c => c.ChapterNumber)
            .Take(allowedAdvanceChapterCount)
            .ToList();

        return allowedAdvancedChapters.Contains(chapter);
    }
}

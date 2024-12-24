namespace DashiToon.Api.Domain.Entities;

public interface IDomainUser
{
    string? Avatar { get; }
    string Id { get; }
    int KanaCoin { get; }
    int KanaGold { get; }
    decimal Revenue { get; }
    DateTimeOffset? MuteUntil { get; }
    DateTimeOffset? RestrictPublishUntil { get; }
    IReadOnlyList<KanaTransaction> Ledgers { get; }
    IReadOnlyList<RevenueTransaction> RevenueTransactions { get; }
    IReadOnlyList<Subscription> Subscriptions { get; }
    IReadOnlyList<Chapter> UnlockedChapters { get; }
    int CurrentDayReadCount();
    bool IsCheckedIn();
    void Checkin();
    bool IsMissionCompleted(Mission mission);
    bool IsMissionCompletable(Mission mission);
    void CompleteMission(Mission mission);
    void AddTransaction(KanaTransaction transaction);
    void AddRevenueTransaction(RevenueTransaction transaction);
    void ReadChapter(int chapterId);
    void BookmarkChapter(int chapterId, int seriesId);
    void UnlockChapter(Chapter chapter);
    void Mute(DateTimeOffset muteUntil);
    void RestrictPublish(DateTimeOffset restrictUntil);
}

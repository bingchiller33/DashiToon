using DashiToon.Api.Domain.Common;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace DashiToon.Api.Infrastructure.Identity;

public class ApplicationUser : IdentityUser, IDomainUser
{
    public string? Avatar { get; private set; }
    public int KanaCoin { get; private set; }
    public int KanaGold { get; private set; }
    public decimal Revenue { get; set; }
    public DateTimeOffset? LastCheckin { get; private set; }
    public DateTimeOffset? MuteUntil { get; private set; }
    public DateTimeOffset? RestrictPublishUntil { get; private set; }

    private readonly List<ReadHistory> _histories = [];
    private readonly List<MissionCompletion> _missionCompletions = [];
    private readonly List<Subscription> _subscriptions = [];
    private readonly List<PurchaseOrder> _purchaseOrders = [];
    private readonly List<SubscriptionOrder> _subscriptionOrders = [];
    private readonly List<KanaTransaction> _ledgers = [];
    private readonly List<Review> _reviews = [];
    private readonly List<Chapter> _unlockedChapters = [];
    private readonly List<Follow> _follows = [];
    private readonly List<RevenueTransaction> _revenueTransactions = [];
    public IReadOnlyList<ReadHistory> Histories => _histories.AsReadOnly();
    public IReadOnlyList<MissionCompletion> MissionCompletions => _missionCompletions.AsReadOnly();
    public IReadOnlyList<Subscription> Subscriptions => _subscriptions.AsReadOnly();
    public IReadOnlyList<PurchaseOrder> PurchaseOrders => _purchaseOrders.AsReadOnly();
    public IReadOnlyList<KanaTransaction> Ledgers => _ledgers.AsReadOnly();
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();
    public IReadOnlyList<Chapter> UnlockedChapters => _unlockedChapters.AsReadOnly();
    public IReadOnlyList<SubscriptionOrder> SubscriptionOrders => _subscriptionOrders.AsReadOnly();
    public IReadOnlyList<Follow> Follows => _follows.AsReadOnly();
    public IReadOnlyList<RevenueTransaction> RevenueTransactions => _revenueTransactions.AsReadOnly();

    public int CurrentDayReadCount()
    {
        DateTimeOffset currentDate = DateTimeOffset.UtcNow;

        DateTimeOffset startOfDay = new(currentDate.Date, TimeSpan.Zero);

        return _histories.Count(x => x.Timestamp > startOfDay);
    }

    public void Checkin()
    {
        if (!IsCheckedIn())
        {
            LastCheckin = DateTimeOffset.UtcNow;
        }
    }

    public bool IsCheckedIn()
    {
        DateTimeOffset currentDate = DateTimeOffset.UtcNow;

        return LastCheckin.HasValue && !IsNewDay(LastCheckin.Value, currentDate);
    }

    private static bool IsNewDay(DateTimeOffset checkDate, DateTimeOffset currentDate)
    {
        return currentDate.Date > checkDate.Date;
    }

    public bool IsMissionCompleted(Mission mission)
    {
        MissionCompletion? missionCompletion = _missionCompletions.FirstOrDefault(mc => mc.MissionId == mission.Id);

        if (missionCompletion is null)
        {
            return false;
        }

        DateTimeOffset currentDate = DateTimeOffset.UtcNow;

        return missionCompletion.LastCompleted.HasValue &&
               missionCompletion.LastCompleted.Value.Date == currentDate.Date;
    }

    public bool IsMissionCompletable(Mission mission)
    {
        return !IsMissionCompleted(mission) && CurrentDayReadCount() >= mission.ReadCount;
    }

    public void CompleteMission(Mission mission)
    {
        MissionCompletion? missionCompletion = _missionCompletions.FirstOrDefault(mc => mc.MissionId == mission.Id);

        if (missionCompletion is null)
        {
            missionCompletion = MissionCompletion.Create(Id, mission.Id);

            _missionCompletions.Add(missionCompletion);
        }

        if (IsMissionCompletable(mission))
        {
            missionCompletion.CompleteMission();
        }
    }

    public void AddTransaction(KanaTransaction transaction)
    {
        _ledgers.Add(transaction);

        KanaCoin = _ledgers
            .Where(t => t.Currency is KanaType.Coin)
            .Sum(t => t.Amount);

        KanaGold = _ledgers
            .Where(t => t.Currency is KanaType.Gold)
            .Sum(t => t.Amount);
    }

    public void ReadChapter(int chapterId)
    {
        if (_histories.All(h => h.ChapterId != chapterId))
        {
            _histories.Add(ReadHistory.Create(Id, chapterId));
        }
    }

    public void BookmarkChapter(int chapterId, int seriesId)
    {
        Follow? follow = _follows.FirstOrDefault(f => f.SeriesId == seriesId);

        if (follow is not null)
        {
            follow.LatestRead(chapterId);
        }
    }

    public void UnlockChapter(Chapter chapter)
    {
        _unlockedChapters.Add(chapter);
    }

    public void Mute(DateTimeOffset muteUntil)
    {
        MuteUntil = muteUntil;
    }

    public void RestrictPublish(DateTimeOffset restrictUntil)
    {
        RestrictPublishUntil = restrictUntil;
    }

    public void AddRevenueTransaction(RevenueTransaction transaction)
    {
        _revenueTransactions.Add(transaction);

        Revenue = _revenueTransactions.Sum(t => t.Amount);
    }
}

public class MissionCompletion : BaseEntity<Guid>
{
    public string UserId { get; private set; } = null!;
    public Guid MissionId { get; private set; }
    public DateTimeOffset? LastCompleted { get; private set; }
    public Mission Mission { get; private set; } = null!;

    private MissionCompletion()
    {
    }

    public static MissionCompletion Create(string userId, Guid missionId)
    {
        return new MissionCompletion { UserId = userId, MissionId = missionId };
    }

    public void CompleteMission()
    {
        LastCompleted = DateTimeOffset.UtcNow;
    }
}

public class ReadHistory : BaseEntity<Guid>
{
    public string UserId { get; private set; } = null!;
    public int ChapterId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public Chapter Chapter { get; private set; } = null!;

    private ReadHistory()
    {
    }

    public static ReadHistory Create(string userId, int chapterId)
    {
        return new ReadHistory { UserId = userId, ChapterId = chapterId, Timestamp = DateTimeOffset.UtcNow };
    }
}

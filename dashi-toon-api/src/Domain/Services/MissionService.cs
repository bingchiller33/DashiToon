namespace DashiToon.Api.Domain.Services;

public class MissionService
{
    public static void DailyCheckin(IDomainUser user)
    {
        if (user.IsCheckedIn())
        {
            throw new AlreadyCheckinException();
        }

        user.Checkin();
        user.AddTransaction(KanaTransaction.Create(KanaType.Coin, TransactionType.Checkin, 100, "Điểm danh"));
    }

    public static void CompleteMission(IDomainUser user, Mission mission)
    {
        if (!mission.IsActive)
        {
            return;
        }

        if (!user.IsMissionCompletable(mission))
        {
            return;
        }

        user.CompleteMission(mission);

        user.AddTransaction(KanaTransaction.Create(
            KanaType.Coin,
            TransactionType.Mission,
            mission.Reward,
            "Đọc chương"));
    }
}

namespace DashiToon.Api.Domain.Entities;

public class Mission : BaseAuditableEntity<Guid>
{
    private Mission()
    {
    }

    public int ReadCount { get; private set; }
    public int Reward { get; private set; }
    public bool IsActive { get; private set; }

    private static void ValidateReadCount(int readCount)
    {
        if (readCount <= 0)
        {
            throw new ArgumentException("ReadCount phải lớn hơn 0");
        }
    }

    private static void ValidateReward(int reward)
    {
        if (reward <= 0)
        {
            throw new ArgumentException("Reward phải lớn hơn 0");
        }
    }

    public static Mission CreateNew(int readCount, int reward, bool isActive)
    {
        ValidateReadCount(readCount);
        ValidateReward(reward);

        return new Mission { Id = Guid.NewGuid(), ReadCount = readCount, Reward = reward, IsActive = isActive };
    }

    public void Update(int readCount, int reward, bool isActive)
    {
        ValidateReadCount(readCount);
        ValidateReward(reward);

        ReadCount = readCount;
        Reward = reward;
        IsActive = isActive;
    }

    public void UpdateStatus(bool isActive)
    {
        IsActive = isActive;
    }
}

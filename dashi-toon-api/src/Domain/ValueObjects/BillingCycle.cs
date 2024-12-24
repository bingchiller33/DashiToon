namespace DashiToon.Api.Domain.ValueObjects;

public class BillingCycle : ValueObject
{
    private BillingCycle()
    {
    }

    public BillingInterval Interval { get; set; }
    public int IntervalCount { get; set; }

    public static BillingCycle CreateNew(BillingInterval interval = BillingInterval.Monthly, int intervalCount = 1)
    {
        if (intervalCount <= 0)
        {
            throw new ArgumentException("Invalid billing duration");
        }

        return new BillingCycle { Interval = interval, IntervalCount = intervalCount };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Interval;
        yield return IntervalCount;
    }
}

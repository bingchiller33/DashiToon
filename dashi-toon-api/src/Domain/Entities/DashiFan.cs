namespace DashiToon.Api.Domain.Entities;

public class DashiFan : BaseAuditableEntity<Guid>
{
    private readonly List<Subscription> _subscriptions = [];

    private DashiFan()
    {
    }

    public string PlanId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Price Price { get; private set; } = null!;

    /// <summary>
    ///     Attributes to customize DashiFan billing cycle. Current version default to monthly billing
    /// </summary>
    public BillingCycle BillingCycle { get; private set; } = null!;

    public string Description { get; private set; } = null!;
    public int Perks { get; private set; }
    public bool IsActive { get; private set; }
    public int SeriesId { get; private set; }
    public Series Series { get; private set; } = null!;
    public IReadOnlyList<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    public static DashiFan Create(
        string name,
        string description,
        int perks,
        decimal amount,
        string currency)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePerks(perks);

        DashiFan tier = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = Price.CreateNew(amount, currency),
            BillingCycle = BillingCycle.CreateNew(),
            Description = description,
            Perks = perks,
            IsActive = true
        };

        tier.AddDomainEvent(new DashiFanCreatedEvent(tier));

        return tier;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length is < 1 or > 255)
        {
            throw new ArgumentException("Name can only have a length between 1 and 255 characters", nameof(name));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrEmpty(description) || description.Length is < 1 or > 255)
        {
            throw new ArgumentException("Description can only have a length between 1 and 255 characters",
                nameof(description));
        }
    }

    private static void ValidatePerks(int perks)
    {
        if (perks <= 0)
        {
            throw new ArgumentException("Advance chapter perks cannot be less than zero.", nameof(perks));
        }
    }

    public void Update(
        string name,
        string description,
        int perks,
        decimal amount
    )
    {
        if (DateTimeOffset.UtcNow - LastModified <= TimeSpan.FromDays(30))
        {
            throw new UpdateDashiFanCoolDownException();
        }

        ValidateName(name);
        ValidateDescription(description);
        ValidatePerks(perks);

        Name = name;
        Description = description;
        Perks = perks;

        AddDomainEvent(new DashiFanUpdatedEvent(this));

        if (Price.Amount != amount)
        {
            Price = Price.CreateNew(amount, Price.Currency);

            AddDomainEvent(new DashiFanPriceUpdatedEvent(this));
        }
    }

    public void ChangeStatus()
    {
        IsActive = !IsActive;

        AddDomainEvent(new DashiFanStatusUpdatedEvent(this));
    }

    public void UpdatePlan(string planId)
    {
        PlanId = planId;
    }
}

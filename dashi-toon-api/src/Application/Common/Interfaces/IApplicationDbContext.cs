using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Genre> Genres { get; }
    DbSet<Series> Series { get; }
    DbSet<Volume> Volumes { get; }
    DbSet<Chapter> Chapters { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<DashiFan> DashiFans { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionOrder> SubscriptionOrders { get; }
    DbSet<Follow> Follows { get; }
    DbSet<Report> Reports { get; }
    DbSet<KanaGoldPack> KanaGoldPacks { get; }
    DbSet<KanaExchangeRate> KanaExchangeRates { get; }
    DbSet<CommissionRate> CommissionRates { get; }
    DbSet<Mission> Missions { get; }
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

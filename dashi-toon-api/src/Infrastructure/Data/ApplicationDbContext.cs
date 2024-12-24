using System.Reflection;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Volume> Volumes => Set<Volume>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<DashiFan> DashiFans => Set<DashiFan>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionOrder> SubscriptionOrders => Set<SubscriptionOrder>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<KanaGoldPack> KanaGoldPacks => Set<KanaGoldPack>();
    public DbSet<KanaExchangeRate> KanaExchangeRates => Set<KanaExchangeRate>();
    public DbSet<CommissionRate> CommissionRates => Set<CommissionRate>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
        });

        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<IdentityUserToken<string>>(b =>
        {
            b.ToTable("UserTokens");
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable("Roles");
        });

        builder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserRole<string>>(b =>
        {
            b.ToTable("UserRoles");
        });
    }
}

using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .OwnsMany(u => u.Ledgers, kt =>
            {
                kt.WithOwner().HasForeignKey(t => t.UserId);

                kt.Property(t => t.Reason).HasMaxLength(500);

                kt.Property(t => t.Id)
                    .ValueGeneratedNever();

                kt.HasOne(x => x.Chapter)
                    .WithMany();
            });

        builder
            .OwnsMany(u => u.RevenueTransactions, kt =>
            {
                kt.WithOwner().HasForeignKey(t => t.AuthorId);

                kt.Property(t => t.Reason).HasMaxLength(500);

                kt.Property(t => t.Id)
                    .ValueGeneratedNever();

                kt.HasOne(t => t.Series)
                    .WithMany();
            });

        builder
            .HasMany(u => u.Subscriptions)
            .WithOne()
            .HasForeignKey(s => s.UserId);

        builder
            .HasMany(u => u.MissionCompletions)
            .WithOne()
            .HasForeignKey(mc => mc.UserId);

        builder
            .HasMany(u => u.Histories)
            .WithOne()
            .HasForeignKey(h => h.UserId);

        builder
            .HasMany(u => u.PurchaseOrders)
            .WithOne()
            .HasForeignKey(po => po.UserId);

        builder
            .HasMany(u => u.SubscriptionOrders)
            .WithOne()
            .HasForeignKey(so => so.UserId);

        builder
            .HasMany(u => u.Reviews)
            .WithOne()
            .HasForeignKey(r => r.UserId);

        builder
            .HasMany(u => u.Follows)
            .WithOne()
            .HasForeignKey(f => f.UserId);

        builder
            .HasMany(u => u.UnlockedChapters)
            .WithMany();
    }
}

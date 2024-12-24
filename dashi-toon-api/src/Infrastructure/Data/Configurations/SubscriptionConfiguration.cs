using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder
            .OwnsMany(u => u.Histories, sh =>
            {
                sh.WithOwner().HasForeignKey("SubscriptionId");
                sh.Property<int>("Id");
                sh.HasKey("Id");
            });
    }
}

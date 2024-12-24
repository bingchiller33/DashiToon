using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class SubscriptionOrderConfiguration : IEntityTypeConfiguration<SubscriptionOrder>
{
    public void Configure(EntityTypeBuilder<SubscriptionOrder> builder)
    {
        builder
            .HasOne(so => so.Subscription)
            .WithMany()
            .HasForeignKey(so => so.SubscriptionId);

        builder.OwnsOne(po => po.Price);
    }
}

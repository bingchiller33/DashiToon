using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class DashiFanConfiguration : IEntityTypeConfiguration<DashiFan>
{
    public void Configure(EntityTypeBuilder<DashiFan> builder)
    {
        builder
            .Property(r => r.Id)
            .ValueGeneratedNever();

        builder
            .Property(df => df.Name)
            .HasMaxLength(255);

        builder
            .Property(df => df.Description)
            .HasMaxLength(255);

        builder.OwnsOne(df => df.Price);
        builder.OwnsOne(df => df.BillingCycle);

        builder
            .HasMany(df => df.Subscriptions)
            .WithOne(sub => sub.Tier)
            .HasForeignKey(df => df.DashiFanId);
    }
}

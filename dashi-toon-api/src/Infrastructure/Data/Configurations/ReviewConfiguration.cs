using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.OwnsMany(r => r.ReviewRates, rr =>
        {
            rr.WithOwner().HasForeignKey("ReviewId");
            rr.Property<Guid>("Id");
            rr.HasKey("Id");

            rr.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey("UserId");
        });

        builder
            .Property(r => r.Content)
            .IsRequired()
            .HasMaxLength(8492);

        builder
            .HasOne(r => r.Series)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.SeriesId)
            .IsRequired();
    }
}

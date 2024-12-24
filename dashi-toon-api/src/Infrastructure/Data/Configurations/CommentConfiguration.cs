using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder
            .OwnsMany(c => c.CommentRates, cr =>
            {
                cr.WithOwner().HasForeignKey("CommentId");
                cr.Property<Guid>("Id");
                cr.HasKey("Id");

                cr.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("UserId");
            });

        builder
            .HasMany(c => c.Replies)
            .WithOne(c => c.ParentComment)
            .HasForeignKey(c => c.ParentCommentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(c => c.Content)
            .HasMaxLength(2048);
    }
}

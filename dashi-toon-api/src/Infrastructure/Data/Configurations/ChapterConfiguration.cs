using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder
            .OwnsMany(c => c.Versions, cv =>
            {
                cv.WithOwner().HasForeignKey("ChapterId");
                cv.Property(x => x.Id)
                    .ValueGeneratedNever();

                cv.Property(c => c.VersionName)
                    .HasMaxLength(255)
                    .IsRequired();

                cv.Property(c => c.Title)
                    .HasMaxLength(255);

                cv.Property(c => c.Thumbnail)
                    .HasMaxLength(100)
                    .IsRequired(false);

                cv.Property(c => c.Note)
                    .HasMaxLength(5000)
                    .IsRequired(false);

                cv.HasKey(x => x.Id);
            });

        builder
            .OwnsMany(c => c.Analytics, a =>
            {
                a.WithOwner().HasForeignKey("ChapterId");
                a.Property<Guid>("Id");
                a.HasKey("Id");
            });

        builder
            .Property(c => c.PublishedVersionId)
            .IsRequired(false);

        builder
            .Property(c => c.PublishedDate)
            .IsRequired(false);

        builder
            .Ignore(c => c.IsAdvanceChapter);
    }
}

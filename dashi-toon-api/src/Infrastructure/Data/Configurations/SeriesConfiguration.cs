using System.Text.Json;
using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder
            .HasMany(s => s.Genres)
            .WithMany();

        builder
            .HasMany(s => s.Volumes)
            .WithOne(v => v.Series)
            .HasForeignKey(v => v.SeriesId)
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.Synopsis)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(s => s.Thumbnail)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(s => s.AlternativeTitles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<string[]>(v, JsonSerializerOptions.Default) ?? Array.Empty<string>())
            .Metadata.SetValueComparer(new ValueComparer<string[]>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray()
            ));

        builder.Property(s => s.Authors)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<string[]>(v, JsonSerializerOptions.Default) ?? Array.Empty<string>())
            .Metadata.SetValueComparer(new ValueComparer<string[]>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray()
            ));

        builder.OwnsMany(s => s.CategoryRatings, cr =>
        {
            cr.WithOwner().HasForeignKey("SeriesId");
            cr.Property<int>("Id");
            cr.HasKey("Id");
        });

        builder.HasMany(s => s.Tiers)
            .WithOne(t => t.Series)
            .HasForeignKey(s => s.SeriesId)
            .IsRequired();
    }
}

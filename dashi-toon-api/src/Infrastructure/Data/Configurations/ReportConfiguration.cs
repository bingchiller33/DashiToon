using System.Text.Json;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.Reported)
            .IsRequired(false);

        builder
            .HasOne(r => r.Comment)
            .WithMany()
            .IsRequired(false);

        builder
            .HasOne(r => r.Review)
            .WithMany()
            .IsRequired(false);

        builder
            .HasOne(r => r.Chapter)
            .WithMany()
            .IsRequired(false);

        builder
            .HasOne(r => r.Series)
            .WithMany()
            .IsRequired(false);

        builder.OwnsOne(r => r.Analysis, a =>
        {
            a.Property(x => x.FlaggedCategories)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<List<CategoryScore>>(v, JsonSerializerOptions.Default) ??
                         Enumerable.Empty<CategoryScore>().ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<CategoryScore>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (v1, v2) => HashCode.Combine(v1, v2.GetHashCode())),
                    c => c.ToList()
                ));
        });
    }
}

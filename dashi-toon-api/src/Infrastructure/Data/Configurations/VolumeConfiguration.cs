using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class VolumeConfiguration : IEntityTypeConfiguration<Volume>
{
    public void Configure(EntityTypeBuilder<Volume> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(100);

        builder
            .Property(x => x.Introduction)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder
            .HasMany(v => v.Chapters)
            .WithOne(c => c.Volume)
            .HasForeignKey(c => c.VolumeId)
            .IsRequired();
    }
}

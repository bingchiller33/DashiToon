using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class ReadHistoryConfiguration : IEntityTypeConfiguration<ReadHistory>
{
    public void Configure(EntityTypeBuilder<ReadHistory> builder)
    {
        builder
            .HasOne(h => h.Chapter)
            .WithMany();
    }
}

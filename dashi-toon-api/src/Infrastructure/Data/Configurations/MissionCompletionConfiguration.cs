using DashiToon.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class MissionCompletionConfiguration : IEntityTypeConfiguration<MissionCompletion>
{
    public void Configure(EntityTypeBuilder<MissionCompletion> builder)
    {
        builder
            .HasOne(mc => mc.Mission)
            .WithMany();
    }
}

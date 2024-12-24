using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashiToon.Api.Infrastructure.Data.Configurations;

public class KanaGoldPackConfiguration : IEntityTypeConfiguration<KanaGoldPack>
{
    public void Configure(EntityTypeBuilder<KanaGoldPack> builder)
    {
        builder.OwnsOne(p => p.Price);
    }
}

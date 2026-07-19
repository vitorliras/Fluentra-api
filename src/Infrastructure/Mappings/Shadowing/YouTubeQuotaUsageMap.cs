using Fluentra.Infrastructure.ExternalServices.Shadowing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentra.Infrastructure.Mappings.Shadowing;

public sealed class YouTubeQuotaUsageMap : IEntityTypeConfiguration<YouTubeQuotaUsage>
{
    public void Configure(EntityTypeBuilder<YouTubeQuotaUsage> builder)
    {
        builder.ToTable("youtube_quota_usage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.HasIndex(x => x.Date)
            .IsUnique();

        builder.Property(x => x.UnitsConsumed)
            .IsRequired();
    }
}

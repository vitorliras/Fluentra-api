using Fluentra.Domain.Entities.Shadowing;
using Fluentra.Domain.ValueObjects.Shadowing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentra.Infrastructure.Mappings.Shadowing;

public sealed class VideoMap : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("videos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.YouTubeVideoId)
            .HasConversion(v => v.Value, v => new YouTubeVideoId(v))
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(x => x.YouTubeVideoId)
            .IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Duration)
            .HasConversion(v => v.Value, v => new VideoDuration(v))
            .IsRequired();

        builder.Property(x => x.ViewCount)
            .IsRequired();

        builder.Property(x => x.LikeCount)
            .IsRequired();

        builder.Navigation(x => x.Scenes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Scenes)
            .WithOne()
            .HasForeignKey("VideoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

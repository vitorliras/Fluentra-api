using Fluentra.Domain.Entities.Shadowing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentra.Infrastructure.Mappings.Shadowing;

public sealed class SceneMap : IEntityTypeConfiguration<Scene>
{
    public void Configure(EntityTypeBuilder<Scene> builder)
    {
        builder.ToTable("scenes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired();

        builder.Property(x => x.TranslationPt);
        builder.Property(x => x.TranslationEs);
        builder.Property(x => x.TranslationFr);

        builder.Property(x => x.SequenceOrder)
            .IsRequired();

        builder.OwnsOne(x => x.Timing, timing =>
        {
            timing.Property(t => t.Start)
                .HasColumnName("start_time")
                .IsRequired();

            timing.Property(t => t.End)
                .HasColumnName("end_time")
                .IsRequired();
        });
    }
}

using Fluentra.Domain.Entities.Shadowing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentra.Infrastructure.Mappings.Shadowing;

public sealed class ScenePracticeProgressMap : IEntityTypeConfiguration<ScenePracticeProgress>
{
    public void Configure(EntityTypeBuilder<ScenePracticeProgress> builder)
    {
        builder.ToTable("scene_practice_progress");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccuracyRate)
            .IsRequired();

        builder.Property(x => x.Passed)
            .IsRequired();

        builder.Property(x => x.EvaluationJson)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.SceneId })
            .IsUnique();
    }
}

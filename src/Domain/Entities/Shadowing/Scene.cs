using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

public sealed class Scene
{
    public int Id { get; private set; }
    public string Text { get; private set; } = null!;
    public string? Translation { get; private set; }
    public SceneTiming Timing { get; private set; } = null!;
    public int SequenceOrder { get; private set; }

    protected Scene()
    {
    }

    internal Scene(string text, string? translation, SceneTiming timing, int sequenceOrder)
    {
        Text = text;
        Translation = translation;
        Timing = timing;
        SequenceOrder = sequenceOrder;
    }
}

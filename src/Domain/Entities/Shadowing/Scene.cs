using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

public sealed class Scene
{
    public int Id { get; private set; }
    public string Text { get; private set; } = null!;
    public SceneTiming Timing { get; private set; } = null!;
    public int SequenceOrder { get; private set; }

    protected Scene()
    {
    }

    internal Scene(string text, SceneTiming timing, int sequenceOrder)
    {
        Text = text;
        Timing = timing;
        SequenceOrder = sequenceOrder;
    }
}

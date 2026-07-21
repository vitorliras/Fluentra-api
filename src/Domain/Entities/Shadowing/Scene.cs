using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

public sealed class Scene
{
    public int Id { get; private set; }
    public string Text { get; private set; } = null!;
    public string? TranslationPt { get; private set; }
    public string? TranslationEs { get; private set; }
    public string? TranslationFr { get; private set; }
    public SceneTiming Timing { get; private set; } = null!;
    public int SequenceOrder { get; private set; }

    protected Scene()
    {
    }

    internal Scene(
        string text,
        string? translationPt,
        string? translationEs,
        string? translationFr,
        SceneTiming timing,
        int sequenceOrder)
    {
        Text = text;
        TranslationPt = translationPt;
        TranslationEs = translationEs;
        TranslationFr = translationFr;
        Timing = timing;
        SequenceOrder = sequenceOrder;
    }
}

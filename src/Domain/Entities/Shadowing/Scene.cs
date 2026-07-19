using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

// Entidade interna do Agregado Video — nunca construída ou mutada fora dele
// (ver Video.AddScene). Uma cena é uma frase praticável, com o trecho de
// áudio/texto correspondente (ver Preparação/20-shadowing/02-segmentacao-em-cenas.md).
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

using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.Shadowing;

// Marca de início/fim de uma cena dentro do vídeo original — vem da legenda
// (ver Preparação/20-shadowing/02-segmentacao-em-cenas.md), nunca calculada aqui.
public sealed record SceneTiming
{
    public TimeSpan Start { get; }
    public TimeSpan End { get; }

    public SceneTiming(TimeSpan start, TimeSpan end)
    {
        if (start < TimeSpan.Zero || end <= start)
            throw new DomainException("InvalidSceneTiming");

        Start = start;
        End = end;
    }
}

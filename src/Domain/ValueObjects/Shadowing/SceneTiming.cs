using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.Shadowing;

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

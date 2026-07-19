using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.Shadowing;

public sealed record VideoDuration
{
    public TimeSpan Value { get; }

    public VideoDuration(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
            throw new DomainException("InvalidVideoDuration");

        Value = value;
    }
}

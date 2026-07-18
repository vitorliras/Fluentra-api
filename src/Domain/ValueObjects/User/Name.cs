using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.User;

public sealed record Name
{
    private const int MaxLength = 100;

    public string Value { get; }

    public Name(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > MaxLength)
            throw new DomainException("InvalidName");

        Value = trimmed;
    }
}

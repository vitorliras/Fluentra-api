using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.User;

public sealed record Username
{
    private const int MaxLength = 30;

    public string Value { get; }

    public Username(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > MaxLength || trimmed.Contains(' '))
            throw new DomainException("InvalidUsername");

        Value = trimmed;
    }
}

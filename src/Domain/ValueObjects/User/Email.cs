using System.Text.RegularExpressions;
using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.User;

public sealed partial record Email
{
    public string Value { get; }

    public Email(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized.Length == 0 || !EmailFormatRegex().IsMatch(normalized))
            throw new DomainException("InvalidEmail");

        Value = normalized;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormatRegex();
}

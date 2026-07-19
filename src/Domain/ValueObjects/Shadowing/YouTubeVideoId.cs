using System.Text.RegularExpressions;
using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.Shadowing;

public sealed partial record YouTubeVideoId
{
    private const int Length = 11;

    public string Value { get; }

    public YouTubeVideoId(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length != Length || !FormatRegex().IsMatch(trimmed))
            throw new DomainException("InvalidYouTubeVideoId");

        Value = trimmed;
    }

    [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
    private static partial Regex FormatRegex();
}

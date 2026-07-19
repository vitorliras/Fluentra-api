using System.Text.RegularExpressions;
using Fluentra.Domain.Exceptions;

namespace Fluentra.Domain.ValueObjects.Shadowing;

// Identificador de vídeo do YouTube — sempre 11 caracteres, do alfabeto
// [A-Za-z0-9_-], formato estável e documentado pela própria plataforma.
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

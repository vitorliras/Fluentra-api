using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Fluentra.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed class LibreTranslateProvider : ITranslationProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LibreTranslateProvider> _logger;

    public LibreTranslateProvider(HttpClient httpClient, ILogger<LibreTranslateProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(string text, string targetLanguage, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "translate",
                new TranslateRequestBody(text, "en", targetLanguage, "text"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LibreTranslate returned {StatusCode}", response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<TranslateResponseBody>(cancellationToken);
            return body?.TranslatedText;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "LibreTranslate request failed");
            return null;
        }
    }

    private sealed record TranslateRequestBody(
        [property: JsonPropertyName("q")] string Q,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("target")] string Target,
        [property: JsonPropertyName("format")] string Format);

    private sealed record TranslateResponseBody(
        [property: JsonPropertyName("translatedText")] string TranslatedText);
}

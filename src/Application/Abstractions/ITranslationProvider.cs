namespace Fluentra.Application.Abstractions;

public interface ITranslationProvider
{
    Task<string?> TranslateAsync(string text, string targetLanguage, CancellationToken cancellationToken = default);
}

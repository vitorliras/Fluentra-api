using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Microsoft.Extensions.Options;
using Whisper.net;
using Whisper.net.Ggml;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed class WhisperSpeechTranscriber : ISpeechTranscriber
{
    private readonly WhisperSettings _settings;
    private readonly Lazy<Task<WhisperFactory>> _factory;

    public WhisperSpeechTranscriber(IOptions<WhisperSettings> settings)
    {
        _settings = settings.Value;
        _factory = new Lazy<Task<WhisperFactory>>(CreateFactoryAsync);
    }

    public async Task<IReadOnlyList<TranscribedWord>> TranscribeAsync(
        Stream wavAudio,
        CancellationToken cancellationToken = default)
    {
        var factory = await _factory.Value;
        using var processor = factory.CreateBuilder()
            .WithLanguage("en")
            .WithTokenTimestamps()
            .Build();

        var words = new List<TranscribedWord>();
        string? currentWordText = null;
        TimeSpan currentWordStart = default;
        TimeSpan currentWordEnd = default;

        await foreach (var segment in processor.ProcessAsync(wavAudio, cancellationToken))
        {
            foreach (var token in segment.Tokens ?? [])
            {
                if (string.IsNullOrEmpty(token.Text) || IsSpecialToken(token.Text))
                    continue;

                var isNewWord = token.Text.StartsWith(' ') || currentWordText is null;
                var cleanText = token.Text.Trim();
                if (cleanText.Length == 0)
                    continue;

                if (isNewWord)
                {
                    if (currentWordText is not null)
                        words.Add(new TranscribedWord(currentWordText, currentWordStart, currentWordEnd));

                    currentWordText = cleanText;
                    currentWordStart = TimeSpan.FromMilliseconds(token.Start * 10);
                    currentWordEnd = TimeSpan.FromMilliseconds(token.End * 10);
                }
                else
                {
                    currentWordText += cleanText;
                    currentWordEnd = TimeSpan.FromMilliseconds(token.End * 10);
                }
            }
        }

        if (currentWordText is not null)
            words.Add(new TranscribedWord(currentWordText, currentWordStart, currentWordEnd));

        return words;
    }

    private static bool IsSpecialToken(string text) => text.StartsWith('[') || text.StartsWith('<');

    private async Task<WhisperFactory> CreateFactoryAsync()
    {
        if (!File.Exists(_settings.ModelPath))
        {
            var directory = Path.GetDirectoryName(_settings.ModelPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
            await using var fileStream = File.Create(_settings.ModelPath);
            await modelStream.CopyToAsync(fileStream);
        }

        return WhisperFactory.FromPath(_settings.ModelPath);
    }
}

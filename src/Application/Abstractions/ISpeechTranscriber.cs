namespace Fluentra.Application.Abstractions;

public interface ISpeechTranscriber
{
    Task<IReadOnlyList<TranscribedWord>> TranscribeAsync(Stream wavAudio, CancellationToken cancellationToken = default);
}

public sealed record TranscribedWord(string Text, TimeSpan Start, TimeSpan End);

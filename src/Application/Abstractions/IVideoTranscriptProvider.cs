namespace Fluentra.Application.Abstractions;

public interface IVideoTranscriptProvider
{
    Task<bool> HasEnglishCaptionsAsync(string youTubeVideoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TranscriptSegment>> GetTranscriptAsync(string youTubeVideoId, CancellationToken cancellationToken = default);
}

public sealed record TranscriptSegment(string Text, TimeSpan Start, TimeSpan End);

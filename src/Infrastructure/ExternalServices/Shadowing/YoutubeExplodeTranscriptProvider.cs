using Fluentra.Application.Abstractions;
using YoutubeExplode;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed class YoutubeExplodeTranscriptProvider : IVideoTranscriptProvider
{
    private readonly YoutubeClient _client = new();

    public async Task<IReadOnlyList<TranscriptSegment>> GetTranscriptAsync(
        string youTubeVideoId,
        CancellationToken cancellationToken = default)
    {
        var trackManifest = await _client.Videos.ClosedCaptions.GetManifestAsync(youTubeVideoId, cancellationToken);
        var trackInfo = trackManifest.TryGetByLanguage("en");
        if (trackInfo is null)
            return [];

        var track = await _client.Videos.ClosedCaptions.GetAsync(trackInfo, cancellationToken);

        return track.Captions
            .Where(caption => !string.IsNullOrWhiteSpace(caption.Text))
            .Select(caption => new TranscriptSegment(caption.Text.Trim(), caption.Offset, caption.Offset + caption.Duration))
            .ToList();
    }
}

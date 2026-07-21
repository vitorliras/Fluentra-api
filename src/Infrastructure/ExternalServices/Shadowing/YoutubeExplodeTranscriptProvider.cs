using System.Text.RegularExpressions;
using Fluentra.Application.Abstractions;
using YoutubeExplode;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed partial class YoutubeExplodeTranscriptProvider : IVideoTranscriptProvider
{
    private const int TargetWordsPerSentence = 14;
    private const int MinWordsForPunctuationFlush = 4;
    private static readonly char[] SentenceTerminators = ['.', '!', '?'];

    private readonly YoutubeClient _client = new();

    public async Task<bool> HasEnglishCaptionsAsync(string youTubeVideoId, CancellationToken cancellationToken = default)
    {
        var trackManifest = await _client.Videos.ClosedCaptions.GetManifestAsync(youTubeVideoId, cancellationToken);
        return trackManifest.TryGetByLanguage("en") is not null;
    }

    public async Task<IReadOnlyList<TranscriptSegment>> GetTranscriptAsync(
        string youTubeVideoId,
        CancellationToken cancellationToken = default)
    {
        var trackManifest = await _client.Videos.ClosedCaptions.GetManifestAsync(youTubeVideoId, cancellationToken);
        var trackInfo = trackManifest.TryGetByLanguage("en");
        if (trackInfo is null)
            return [];

        var track = await _client.Videos.ClosedCaptions.GetAsync(trackInfo, cancellationToken);

        var cleaned = track.Captions
            .Select(caption => (Text: CleanCaptionText(caption.Text), caption.Offset, caption.Duration))
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
            .ToList();

        return MergeIntoSentences(CapOverlappingEnds(cleaned));
    }

    private static List<(string Text, TimeSpan Offset, TimeSpan End)> CapOverlappingEnds(
        List<(string Text, TimeSpan Offset, TimeSpan Duration)> captions)
    {
        var result = new List<(string Text, TimeSpan Offset, TimeSpan End)>();

        for (var i = 0; i < captions.Count; i++)
        {
            var reportedEnd = captions[i].Offset + captions[i].Duration;
            var nextStart = i + 1 < captions.Count ? captions[i + 1].Offset : (TimeSpan?)null;
            var end = nextStart is not null && nextStart.Value < reportedEnd ? nextStart.Value : reportedEnd;

            result.Add((captions[i].Text, captions[i].Offset, end));
        }

        return result;
    }

    private static string CleanCaptionText(string text)
    {
        var cleaned = BracketTagRegex().Replace(text, " ");
        cleaned = SpeakerMarkerRegex().Replace(cleaned, " ");
        cleaned = WhitespaceRegex().Replace(cleaned, " ").Trim();
        return cleaned;
    }

    private static List<TranscriptSegment> MergeIntoSentences(List<(string Text, TimeSpan Offset, TimeSpan End)> captions)
    {
        var sentences = new List<TranscriptSegment>();
        var bufferText = new List<string>();
        var bufferWordCount = 0;
        TimeSpan? bufferStart = null;
        var bufferEnd = TimeSpan.Zero;

        void FlushBuffer()
        {
            if (bufferText.Count == 0 || bufferStart is null)
                return;

            sentences.Add(new TranscriptSegment(string.Join(' ', bufferText), bufferStart.Value, bufferEnd));
            bufferText.Clear();
            bufferWordCount = 0;
            bufferStart = null;
        }

        foreach (var caption in captions)
        {
            bufferStart ??= caption.Offset;
            bufferText.Add(caption.Text);
            bufferWordCount += caption.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            bufferEnd = caption.End;

            var endsSentence = SentenceTerminators.Contains(caption.Text[^1]);
            var reachedTarget = bufferWordCount >= TargetWordsPerSentence;

            if ((endsSentence && bufferWordCount >= MinWordsForPunctuationFlush) || reachedTarget)
                FlushBuffer();
        }

        FlushBuffer();

        return sentences;
    }

    [GeneratedRegex(@"\[[^\]]*\]|\([^)]*\)")]
    private static partial Regex BracketTagRegex();

    [GeneratedRegex(@">>+\s*")]
    private static partial Regex SpeakerMarkerRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}

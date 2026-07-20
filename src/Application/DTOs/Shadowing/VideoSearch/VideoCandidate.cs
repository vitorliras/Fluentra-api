namespace Fluentra.Application.DTOs.Shadowing.VideoSearch;

public sealed record VideoCandidate(
    string YouTubeVideoId,
    string Title,
    string ThumbnailUrl,
    TimeSpan Duration,
    long ViewCount,
    long LikeCount,
    bool HasCaptions,
    string Language);

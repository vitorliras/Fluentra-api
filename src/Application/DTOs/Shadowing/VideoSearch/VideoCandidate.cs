namespace Fluentra.Application.DTOs.Shadowing.VideoSearch;

public sealed record VideoCandidate(
    string YouTubeVideoId,
    string Title,
    TimeSpan Duration,
    long ViewCount,
    long LikeCount,
    bool HasCaptions,
    string Language);

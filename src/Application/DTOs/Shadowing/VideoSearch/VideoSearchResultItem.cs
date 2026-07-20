namespace Fluentra.Application.DTOs.Shadowing.VideoSearch;

public sealed record VideoSearchResultItem(
    string YouTubeVideoId,
    string Title,
    string ThumbnailUrl,
    TimeSpan Duration,
    long ViewCount,
    long LikeCount,
    string PopularityTier);

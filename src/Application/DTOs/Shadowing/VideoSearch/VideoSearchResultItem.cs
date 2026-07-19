namespace Fluentra.Application.DTOs.Shadowing.VideoSearch;

public sealed record VideoSearchResultItem(
    string YouTubeVideoId,
    string Title,
    TimeSpan Duration,
    long ViewCount,
    string PopularityTier);

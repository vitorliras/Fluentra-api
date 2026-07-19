namespace Fluentra.Application.DTOs.Shadowing.VideoSearch;

public sealed record SearchVideosResponse(IReadOnlyList<VideoSearchResultItem> Videos, bool QuotaNearLimit);

namespace Fluentra.Application.DTOs.Shadowing.History;

public sealed record ShadowingHistoryResponse(IReadOnlyList<ShadowingHistoryItem> Items);

public sealed record ShadowingHistoryItem(
    string YouTubeVideoId,
    string Title,
    string ThumbnailUrl,
    int CompletedScenes,
    int TotalScenes);

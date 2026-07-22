using Fluentra.Domain.Entities.Shadowing;

namespace Fluentra.Domain.Interfaces.Shadowing;

public sealed record InProgressVideoSummary(
    string YouTubeVideoId,
    string Title,
    string ThumbnailUrl,
    int CompletedScenes,
    int TotalScenes);

public interface IScenePracticeProgressRepository
{
    Task<IReadOnlyDictionary<int, ScenePracticeProgress>> GetBySceneIdsAsync(
        int userId, IReadOnlyList<int> sceneIds, CancellationToken cancellationToken = default);

    Task UpsertAsync(
        int userId, int sceneId, double accuracyRate, bool passed, string evaluationJson,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InProgressVideoSummary>> GetInProgressVideosAsync(
        int userId, CancellationToken cancellationToken = default);
}

using Fluentra.Domain.Entities.Shadowing;
using Fluentra.Domain.Interfaces.Shadowing;
using Fluentra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainVideo = Fluentra.Domain.Entities.Shadowing.Video;

namespace Fluentra.Infrastructure.Repositories.Shadowing;

public sealed class ScenePracticeProgressRepository : IScenePracticeProgressRepository
{
    private readonly AppDbContext _context;

    public ScenePracticeProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<int, ScenePracticeProgress>> GetBySceneIdsAsync(
        int userId, IReadOnlyList<int> sceneIds, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Set<ScenePracticeProgress>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && sceneIds.Contains(x.SceneId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.SceneId);
    }

    public async Task UpsertAsync(
        int userId, int sceneId, double accuracyRate, bool passed, string evaluationJson,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<ScenePracticeProgress>()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SceneId == sceneId, cancellationToken);

        if (existing is null)
        {
            await _context.Set<ScenePracticeProgress>().AddAsync(
                new ScenePracticeProgress(userId, sceneId, accuracyRate, passed, evaluationJson), cancellationToken);
        }
        else
        {
            existing.UpdateEvaluation(accuracyRate, passed, evaluationJson);
        }
    }

    public async Task<IReadOnlyList<InProgressVideoSummary>> GetInProgressVideosAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        var videos = await _context.Set<DomainVideo>()
            .Include(v => v.Scenes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sceneIds = videos.SelectMany(v => v.Scenes.Select(s => s.Id)).ToList();

        var passedSceneIds = await _context.Set<ScenePracticeProgress>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Passed && sceneIds.Contains(x.SceneId))
            .Select(x => x.SceneId)
            .ToListAsync(cancellationToken);

        var passedSet = passedSceneIds.ToHashSet();
        var summaries = new List<InProgressVideoSummary>();

        foreach (var video in videos)
        {
            var total = video.Scenes.Count;
            var completed = video.Scenes.Count(s => passedSet.Contains(s.Id));

            if (completed > 0 && completed < total)
            {
                summaries.Add(new InProgressVideoSummary(
                    video.YouTubeVideoId.Value, video.Title, video.ThumbnailUrl, completed, total));
            }
        }

        return summaries;
    }
}

using Fluentra.Domain.Exceptions;
using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

public sealed class Video
{
    private readonly List<Scene> _scenes = [];

    public int Id { get; private set; }
    public YouTubeVideoId YouTubeVideoId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string ThumbnailUrl { get; private set; } = null!;
    public VideoDuration Duration { get; private set; } = null!;
    public long ViewCount { get; private set; }
    public long LikeCount { get; private set; }
    public IReadOnlyCollection<Scene> Scenes => _scenes;

    protected Video()
    {
    }

    public Video(
        YouTubeVideoId youTubeVideoId,
        string title,
        string thumbnailUrl,
        VideoDuration duration,
        long viewCount,
        long likeCount)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("InvalidVideoTitle");

        YouTubeVideoId = youTubeVideoId;
        Title = title.Trim();
        ThumbnailUrl = thumbnailUrl ?? string.Empty;
        Duration = duration;
        ViewCount = viewCount;
        LikeCount = likeCount;
    }

    public void AddScene(string text, string? translationPt, string? translationEs, string? translationFr, SceneTiming timing)
    {
        var sequenceOrder = _scenes.Count + 1;
        _scenes.Add(new Scene(text, translationPt, translationEs, translationFr, timing, sequenceOrder));
    }
}

using Fluentra.Domain.Exceptions;
using Fluentra.Domain.ValueObjects.Shadowing;

namespace Fluentra.Domain.Entities.Shadowing;

// Agregado raiz do módulo Shadowing — Scene só é acessada/mutada através
// daqui (ver AddScene), nunca diretamente de fora. Representa um vídeo do
// YouTube já importado e segmentado em cenas para prática de fala.
public sealed class Video
{
    private readonly List<Scene> _scenes = [];

    public int Id { get; private set; }
    public YouTubeVideoId YouTubeVideoId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public VideoDuration Duration { get; private set; } = null!;
    public long ViewCount { get; private set; }
    public long LikeCount { get; private set; }
    public IReadOnlyCollection<Scene> Scenes => _scenes;

    protected Video()
    {
    }

    public Video(YouTubeVideoId youTubeVideoId, string title, VideoDuration duration, long viewCount, long likeCount)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("InvalidVideoTitle");

        YouTubeVideoId = youTubeVideoId;
        Title = title.Trim();
        Duration = duration;
        ViewCount = viewCount;
        LikeCount = likeCount;
    }

    // A ordem é sempre atribuída aqui, pela Raiz — nunca informada de fora —
    // para garantir que a sequência de cenas nunca fique inconsistente.
    public void AddScene(string text, SceneTiming timing)
    {
        var sequenceOrder = _scenes.Count + 1;
        _scenes.Add(new Scene(text, timing, sequenceOrder));
    }
}

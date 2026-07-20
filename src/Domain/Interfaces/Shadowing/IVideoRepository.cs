using Fluentra.Domain.Entities.Shadowing;

namespace Fluentra.Domain.Interfaces.Shadowing;

public interface IVideoRepository
{
    Task<bool> AddAsync(Video video);
    Task<Video?> GetByYouTubeVideoIdAsync(string youTubeVideoId);
}

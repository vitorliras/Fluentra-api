using Fluentra.Application.DTOs.Shadowing.VideoSearch;

namespace Fluentra.Application.Abstractions;

public interface IVideoSearchProvider
{
    Task<IReadOnlyList<VideoCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task<VideoCandidate?> GetByIdAsync(string youTubeVideoId, CancellationToken cancellationToken = default);
}

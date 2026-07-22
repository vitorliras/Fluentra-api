using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Shadowing.History;
using Fluentra.Domain.Interfaces.Shadowing;
using Fluentra.Shared.Results;

namespace Fluentra.Application.UseCases.Shadowing.Video;

public sealed class GetShadowingHistoryUseCase : IUseCaseWithoutRequest<ShadowingHistoryResponse>
{
    private readonly IScenePracticeProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUser;

    public GetShadowingHistoryUseCase(IScenePracticeProgressRepository progressRepository, ICurrentUserService currentUser)
    {
        _progressRepository = progressRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ShadowingHistoryResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await _progressRepository.GetInProgressVideosAsync(_currentUser.UserId, cancellationToken);

        var items = summaries
            .Select(x => new ShadowingHistoryItem(x.YouTubeVideoId, x.Title, x.ThumbnailUrl, x.CompletedScenes, x.TotalScenes))
            .ToList();

        return Result<ShadowingHistoryResponse>.Success(new ShadowingHistoryResponse(items));
    }
}

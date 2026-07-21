using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoImport;
using Fluentra.Domain.Exceptions;
using Fluentra.Domain.Interfaces.Shadowing;
using Fluentra.Domain.ValueObjects.Shadowing;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;
using Microsoft.Extensions.Options;
using DomainVideo = Fluentra.Domain.Entities.Shadowing.Video;

namespace Fluentra.Application.UseCases.Shadowing.Video;

public sealed class ImportVideoUseCase : IUseCase<ImportVideoRequest, ImportVideoResponse>
{
    private readonly IVideoSearchProvider _videoSearchProvider;
    private readonly IVideoTranscriptProvider _transcriptProvider;
    private readonly IVideoRepository _videoRepository;
    private readonly IYouTubeQuotaTracker _quotaTracker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly YouTubeSettings _settings;

    public ImportVideoUseCase(
        IVideoSearchProvider videoSearchProvider,
        IVideoTranscriptProvider transcriptProvider,
        IVideoRepository videoRepository,
        IYouTubeQuotaTracker quotaTracker,
        IUnitOfWork unitOfWork,
        IOptions<YouTubeSettings> settings)
    {
        _videoSearchProvider = videoSearchProvider;
        _transcriptProvider = transcriptProvider;
        _videoRepository = videoRepository;
        _quotaTracker = quotaTracker;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
    }

    public async Task<Result<ImportVideoResponse>> ExecuteAsync(
        ImportVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        YouTubeVideoId videoId;
        try
        {
            videoId = new YouTubeVideoId(request.YouTubeVideoId);
        }
        catch (DomainException)
        {
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.InvalidVideoUrl));
        }

        var existing = await _videoRepository.GetByYouTubeVideoIdAsync(videoId.Value);
        if (existing is not null)
            return Result<ImportVideoResponse>.Success(
                new ImportVideoResponse(existing.Id, existing.Title, ToSceneDtos(existing.Scenes)));

        var consumption = await _quotaTracker.TryConsumeAsync(_settings.LookupCostUnits, cancellationToken);
        if (!consumption.Allowed)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.YouTubeQuotaExhausted));

        var candidate = await _videoSearchProvider.GetByIdAsync(videoId.Value, cancellationToken);
        if (candidate is null)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.VideoNotFound));

        var hasCaptions = await _transcriptProvider.HasEnglishCaptionsAsync(videoId.Value, cancellationToken);
        if (!hasCaptions)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.VideoNotEligible));

        var transcript = await _transcriptProvider.GetTranscriptAsync(videoId.Value, cancellationToken);
        if (transcript.Count == 0)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.TranscriptUnavailable));

        var video = new DomainVideo(
            videoId, candidate.Title, candidate.ThumbnailUrl, new VideoDuration(candidate.Duration),
            candidate.ViewCount, candidate.LikeCount);

        foreach (var segment in transcript)
            video.AddScene(segment.Text, new SceneTiming(segment.Start, segment.End));

        var added = await _videoRepository.AddAsync(video);
        if (!added)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.ImportFailed));

        var committed = await _unitOfWork.CommitAsync(cancellationToken);
        if (!committed)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.PersistenceError));

        return Result<ImportVideoResponse>.Success(
            new ImportVideoResponse(video.Id, video.Title, ToSceneDtos(video.Scenes)));
    }

    private static IReadOnlyList<SceneDto> ToSceneDtos(IEnumerable<Domain.Entities.Shadowing.Scene> scenes) =>
        scenes
            .OrderBy(x => x.SequenceOrder)
            .Select(x => new SceneDto(x.Id, x.Text, x.Timing.Start.TotalSeconds, x.Timing.End.TotalSeconds, x.SequenceOrder))
            .ToList();
}

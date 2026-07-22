using System.Text.Json;
using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;
using Fluentra.Application.DTOs.Shadowing.VideoImport;
using Fluentra.Domain.Entities.Shadowing;
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
    private static readonly string[] TranslationTargetLanguages = ["pt", "es", "fr"];

    private readonly IVideoSearchProvider _videoSearchProvider;
    private readonly IVideoTranscriptProvider _transcriptProvider;
    private readonly ITranslationProvider _translationProvider;
    private readonly IVideoRepository _videoRepository;
    private readonly IScenePracticeProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IYouTubeQuotaTracker _quotaTracker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly YouTubeSettings _settings;

    public ImportVideoUseCase(
        IVideoSearchProvider videoSearchProvider,
        IVideoTranscriptProvider transcriptProvider,
        ITranslationProvider translationProvider,
        IVideoRepository videoRepository,
        IScenePracticeProgressRepository progressRepository,
        ICurrentUserService currentUser,
        IYouTubeQuotaTracker quotaTracker,
        IUnitOfWork unitOfWork,
        IOptions<YouTubeSettings> settings)
    {
        _videoSearchProvider = videoSearchProvider;
        _transcriptProvider = transcriptProvider;
        _translationProvider = translationProvider;
        _videoRepository = videoRepository;
        _progressRepository = progressRepository;
        _currentUser = currentUser;
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
        {
            var sceneIds = existing.Scenes.Select(x => x.Id).ToList();
            var progress = await _progressRepository.GetBySceneIdsAsync(_currentUser.UserId, sceneIds, cancellationToken);

            return Result<ImportVideoResponse>.Success(
                new ImportVideoResponse(existing.Id, existing.Title, ToSceneDtos(existing.Scenes, progress)));
        }

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
        {
            var translations = new string?[TranslationTargetLanguages.Length];
            for (var i = 0; i < TranslationTargetLanguages.Length; i++)
                translations[i] = await _translationProvider.TranslateAsync(segment.Text, TranslationTargetLanguages[i], cancellationToken);

            video.AddScene(segment.Text, translations[0], translations[1], translations[2], new SceneTiming(segment.Start, segment.End));
        }

        var added = await _videoRepository.AddAsync(video);
        if (!added)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.ImportFailed));

        var committed = await _unitOfWork.CommitAsync(cancellationToken);
        if (!committed)
            return Result<ImportVideoResponse>.Failure(Error.From(ShadowingErrorCodes.PersistenceError));

        return Result<ImportVideoResponse>.Success(
            new ImportVideoResponse(video.Id, video.Title, ToSceneDtos(video.Scenes, new Dictionary<int, ScenePracticeProgress>())));
    }

    private static IReadOnlyList<SceneDto> ToSceneDtos(
        IEnumerable<Domain.Entities.Shadowing.Scene> scenes,
        IReadOnlyDictionary<int, ScenePracticeProgress> progress) =>
        scenes
            .OrderBy(x => x.SequenceOrder)
            .Select(x =>
            {
                var hasProgress = progress.TryGetValue(x.Id, out var sceneProgress);
                var lastEvaluation = hasProgress
                    ? JsonSerializer.Deserialize<EvaluatePronunciationResponse>(sceneProgress!.EvaluationJson)
                    : null;

                return new SceneDto(
                    x.Id,
                    x.Text,
                    BuildTranslations(x),
                    x.Timing.Start.TotalSeconds,
                    x.Timing.End.TotalSeconds,
                    x.SequenceOrder,
                    hasProgress && sceneProgress!.Passed,
                    lastEvaluation);
            })
            .ToList();

    private static IReadOnlyDictionary<string, string> BuildTranslations(Domain.Entities.Shadowing.Scene scene)
    {
        var translations = new Dictionary<string, string>();
        if (scene.TranslationPt is not null) translations["pt"] = scene.TranslationPt;
        if (scene.TranslationEs is not null) translations["es"] = scene.TranslationEs;
        if (scene.TranslationFr is not null) translations["fr"] = scene.TranslationFr;
        return translations;
    }
}

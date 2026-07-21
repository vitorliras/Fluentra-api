using System.Text.RegularExpressions;
using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Domain.Exceptions;
using Fluentra.Domain.ValueObjects.Shadowing;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;
using Microsoft.Extensions.Options;

namespace Fluentra.Application.UseCases.Shadowing.Video;

public sealed partial class GetVideoByUrlUseCase : IUseCase<GetVideoByUrlRequest, VideoSearchResultItem>
{
    private const long SmallTierUpperBound = 10_000;
    private const long MediumTierUpperBound = 100_000;
    private const long LargeTierUpperBound = 1_000_000;

    private readonly IVideoSearchProvider _videoSearchProvider;
    private readonly IVideoTranscriptProvider _transcriptProvider;
    private readonly IYouTubeQuotaTracker _quotaTracker;
    private readonly YouTubeSettings _settings;

    public GetVideoByUrlUseCase(
        IVideoSearchProvider videoSearchProvider,
        IVideoTranscriptProvider transcriptProvider,
        IYouTubeQuotaTracker quotaTracker,
        IOptions<YouTubeSettings> settings)
    {
        _videoSearchProvider = videoSearchProvider;
        _transcriptProvider = transcriptProvider;
        _quotaTracker = quotaTracker;
        _settings = settings.Value;
    }

    public async Task<Result<VideoSearchResultItem>> ExecuteAsync(
        GetVideoByUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        string videoId;
        try
        {
            videoId = new YouTubeVideoId(ExtractVideoId(request.Url)).Value;
        }
        catch (DomainException)
        {
            return Result<VideoSearchResultItem>.Failure(Error.From(ShadowingErrorCodes.InvalidVideoUrl));
        }

        var consumption = await _quotaTracker.TryConsumeAsync(_settings.LookupCostUnits, cancellationToken);
        if (!consumption.Allowed)
            return Result<VideoSearchResultItem>.Failure(Error.From(ShadowingErrorCodes.YouTubeQuotaExhausted));

        var candidate = await _videoSearchProvider.GetByIdAsync(videoId, cancellationToken);
        if (candidate is null)
            return Result<VideoSearchResultItem>.Failure(Error.From(ShadowingErrorCodes.VideoNotFound));

        var hasCaptions = await _transcriptProvider.HasEnglishCaptionsAsync(videoId, cancellationToken);
        if (!hasCaptions)
            return Result<VideoSearchResultItem>.Failure(Error.From(ShadowingErrorCodes.VideoNotEligible));

        var tier = ClassifyPopularity(candidate.ViewCount);

        return Result<VideoSearchResultItem>.Success(new VideoSearchResultItem(
            candidate.YouTubeVideoId, candidate.Title, candidate.ThumbnailUrl, candidate.Duration,
            candidate.ViewCount, candidate.LikeCount, tier));
    }

    private static string ExtractVideoId(string url)
    {
        var trimmed = url.Trim();
        var match = UrlPatternRegex().Match(trimmed);

        return match.Success ? match.Groups[1].Value : trimmed;
    }

    private static string ClassifyPopularity(long viewCount) => viewCount switch
    {
        <= SmallTierUpperBound => "Pequena",
        <= MediumTierUpperBound => "Media",
        <= LargeTierUpperBound => "Grande",
        _ => "Viral",
    };

    [GeneratedRegex(@"(?:youtube\.com/watch\?v=|youtube\.com/embed/|youtu\.be/)([A-Za-z0-9_-]{11})")]
    private static partial Regex UrlPatternRegex();
}

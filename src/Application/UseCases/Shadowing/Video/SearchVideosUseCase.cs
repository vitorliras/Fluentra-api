using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;
using Microsoft.Extensions.Options;

namespace Fluentra.Application.UseCases.Shadowing.Video;

public sealed class SearchVideosUseCase : IUseCase<SearchVideosRequest, SearchVideosResponse>
{
    private const long MinimumViewCount = 1_000;
    private const long SmallTierUpperBound = 10_000;
    private const long MediumTierUpperBound = 100_000;
    private const long LargeTierUpperBound = 1_000_000;
    private const int FixedSlotsPerTier = 3;
    private const int MaxResults = 15;
    private static readonly TimeSpan DurationTolerance = TimeSpan.FromMinutes(2);

    private readonly IVideoSearchProvider _videoSearchProvider;
    private readonly IYouTubeQuotaTracker _quotaTracker;
    private readonly YouTubeSettings _settings;

    public SearchVideosUseCase(
        IVideoSearchProvider videoSearchProvider,
        IYouTubeQuotaTracker quotaTracker,
        IOptions<YouTubeSettings> settings)
    {
        _videoSearchProvider = videoSearchProvider;
        _quotaTracker = quotaTracker;
        _settings = settings.Value;
    }

    public async Task<Result<SearchVideosResponse>> ExecuteAsync(
        SearchVideosRequest request,
        CancellationToken cancellationToken = default)
    {
        var consumption = await _quotaTracker.TryConsumeAsync(_settings.SearchCostUnits, cancellationToken);
        if (!consumption.Allowed)
            return Result<SearchVideosResponse>.Failure(Error.From(ShadowingErrorCodes.YouTubeQuotaExhausted));

        var candidates = await _videoSearchProvider.SearchAsync(request.Subject, cancellationToken);
        var desiredDuration = TimeSpan.FromMinutes(request.DesiredDurationMinutes);

        var eligible = candidates
            .Where(c => c.HasCaptions)
            .Where(c => c.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            .Where(c => c.ViewCount >= MinimumViewCount)
            .Where(c => IsWithinDurationTolerance(c.Duration, desiredDuration))
            .ToList();

        if (eligible.Count == 0)
            return Result<SearchVideosResponse>.Success(new SearchVideosResponse([], consumption.NearLimit));

        var scored = eligible
            .Select(c => new ScoredCandidate(c, ClassifyPopularity(c.ViewCount), ComputeQualityScore(c)))
            .OrderByDescending(x => x.QualityScore)
            .ToList();

        var selected = BuildFinalList(scored);

        var videos = selected
            .Select(x => new VideoSearchResultItem(
                x.Candidate.YouTubeVideoId,
                x.Candidate.Title,
                x.Candidate.Duration,
                x.Candidate.ViewCount,
                x.Tier))
            .ToList();

        return Result<SearchVideosResponse>.Success(new SearchVideosResponse(videos, consumption.NearLimit));
    }

    private static bool IsWithinDurationTolerance(TimeSpan actual, TimeSpan desired) =>
        (actual - desired).Duration() <= DurationTolerance;

    private static string ClassifyPopularity(long viewCount) => viewCount switch
    {
        <= SmallTierUpperBound => "Pequena",
        <= MediumTierUpperBound => "Media",
        <= LargeTierUpperBound => "Grande",
        _ => "Viral",
    };

    // log10 das views amortece diferenças grandes, combinado com a proporção de curtidas
    private static double ComputeQualityScore(VideoCandidate candidate)
    {
        var viewScore = Math.Log10(candidate.ViewCount);
        var likeRatio = candidate.ViewCount == 0 ? 0 : (double)candidate.LikeCount / candidate.ViewCount;

        return viewScore + (likeRatio * 10);
    }

    private static List<ScoredCandidate> BuildFinalList(List<ScoredCandidate> scoredDescending)
    {
        var result = new List<ScoredCandidate>();
        var remaining = new List<ScoredCandidate>(scoredDescending);

        foreach (var tier in new[] { "Viral", "Grande", "Media" })
        {
            var picks = remaining.Where(x => x.Tier == tier).Take(FixedSlotsPerTier).ToList();
            result.AddRange(picks);

            foreach (var pick in picks)
                remaining.Remove(pick);
        }

        var flexibleSlots = MaxResults - result.Count;
        result.AddRange(remaining.OrderByDescending(x => x.QualityScore).Take(flexibleSlots));

        return result.OrderByDescending(x => x.QualityScore).ToList();
    }

    private sealed record ScoredCandidate(VideoCandidate Candidate, string Tier, double QualityScore);
}

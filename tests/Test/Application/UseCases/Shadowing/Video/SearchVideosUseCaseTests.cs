using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Application.UseCases.Shadowing.Video;
using Fluentra.Shared.Messages;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Fluentra.Test.Application.UseCases.Shadowing.Video;

public sealed class SearchVideosUseCaseTests
{
    private readonly Mock<IVideoSearchProvider> _videoSearchProvider = new();
    private readonly Mock<IYouTubeQuotaTracker> _quotaTracker = new();

    private SearchVideosUseCase CreateSut() =>
        new(_videoSearchProvider.Object, _quotaTracker.Object, Options.Create(new YouTubeSettings()));

    private static SearchVideosRequest ValidRequest() => new("programação", 10);

    private static VideoCandidate Candidate(
        string id = "abcdefghijk",
        TimeSpan? duration = null,
        long viewCount = 50_000,
        long likeCount = 2_500,
        bool hasCaptions = true,
        string language = "en") =>
        new(id, "Some Video", duration ?? TimeSpan.FromMinutes(10), viewCount, likeCount, hasCaptions, language);

    [Fact]
    public async Task Should_Return_Failure_When_Quota_Is_Exhausted()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(false, true));

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.YouTubeQuotaExhausted);
        _videoSearchProvider.Verify(x => x.SearchAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_No_Candidate_Is_Eligible()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.SearchAsync(It.IsAny<string>(), default))
            .ReturnsAsync([Candidate(hasCaptions: false)]);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Videos.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Discard_Candidate_Below_Minimum_View_Count()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.SearchAsync(It.IsAny<string>(), default))
            .ReturnsAsync([Candidate(viewCount: 999)]);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Videos.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Discard_Candidate_Outside_Duration_Tolerance()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.SearchAsync(It.IsAny<string>(), default))
            .ReturnsAsync([Candidate(duration: TimeSpan.FromMinutes(30))]);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Videos.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Classify_Eligible_Videos_By_Popularity_Tier()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.SearchAsync(It.IsAny<string>(), default))
            .ReturnsAsync(
            [
                Candidate("smallvideo01", viewCount: 5_000),
                Candidate("mediumvideo1", viewCount: 50_000),
                Candidate("largevideoid", viewCount: 500_000),
                Candidate("viralvideo01", viewCount: 5_000_000),
            ]);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Videos.Count.ShouldBe(4);
        result.Value.Videos.Single(v => v.YouTubeVideoId == "smallvideo01").PopularityTier.ShouldBe("Pequena");
        result.Value.Videos.Single(v => v.YouTubeVideoId == "mediumvideo1").PopularityTier.ShouldBe("Media");
        result.Value.Videos.Single(v => v.YouTubeVideoId == "largevideoid").PopularityTier.ShouldBe("Grande");
        result.Value.Videos.Single(v => v.YouTubeVideoId == "viralvideo01").PopularityTier.ShouldBe("Viral");
    }

    [Fact]
    public async Task Should_Propagate_Quota_Near_Limit_Flag()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, true));
        _videoSearchProvider.Setup(x => x.SearchAsync(It.IsAny<string>(), default))
            .ReturnsAsync([Candidate()]);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value!.QuotaNearLimit.ShouldBeTrue();
    }
}

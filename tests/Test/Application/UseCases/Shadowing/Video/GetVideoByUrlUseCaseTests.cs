using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Application.UseCases.Shadowing.Video;
using Fluentra.Shared.Messages;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Fluentra.Test.Application.UseCases.Shadowing.Video;

public sealed class GetVideoByUrlUseCaseTests
{
    private readonly Mock<IVideoSearchProvider> _videoSearchProvider = new();
    private readonly Mock<IVideoTranscriptProvider> _transcriptProvider = new();
    private readonly Mock<IYouTubeQuotaTracker> _quotaTracker = new();

    public GetVideoByUrlUseCaseTests()
    {
        _transcriptProvider.Setup(x => x.HasEnglishCaptionsAsync(It.IsAny<string>(), default)).ReturnsAsync(true);
    }

    private GetVideoByUrlUseCase CreateSut() =>
        new(_videoSearchProvider.Object, _transcriptProvider.Object, _quotaTracker.Object, Options.Create(new YouTubeSettings()));

    private static VideoCandidate Candidate(long viewCount = 50_000) =>
        new("dQw4w9WgXcQ", "Some Video", "https://example.com/thumb.jpg", TimeSpan.FromMinutes(5), viewCount, 1_000);

    [Fact]
    public async Task Should_Return_Failure_When_Url_Has_No_Valid_Video_Id()
    {
        var result = await CreateSut().ExecuteAsync(new GetVideoByUrlRequest("https://example.com/not-a-video"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.InvalidVideoUrl);
        _quotaTracker.Verify(x => x.TryConsumeAsync(It.IsAny<int>(), default), Times.Never);
    }

    [Fact]
    public async Task Should_Extract_Id_From_Watch_Url()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync("dQw4w9WgXcQ", default))
            .ReturnsAsync(Candidate());

        var result = await CreateSut().ExecuteAsync(
            new GetVideoByUrlRequest("https://www.youtube.com/watch?v=dQw4w9WgXcQ"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.YouTubeVideoId.ShouldBe("dQw4w9WgXcQ");
    }

    [Fact]
    public async Task Should_Reject_Url_With_Video_Id_Longer_Than_Eleven_Characters()
    {
        var result = await CreateSut().ExecuteAsync(
            new GetVideoByUrlRequest("https://www.youtube.com/watch?v=MFsYaRnrcPQe"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.InvalidVideoUrl);
        _videoSearchProvider.Verify(x => x.GetByIdAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Should_Accept_Bare_Video_Id()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync("dQw4w9WgXcQ", default))
            .ReturnsAsync(Candidate());

        var result = await CreateSut().ExecuteAsync(new GetVideoByUrlRequest("dQw4w9WgXcQ"));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Return_Failure_When_Quota_Is_Exhausted()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(false, true));

        var result = await CreateSut().ExecuteAsync(
            new GetVideoByUrlRequest("https://youtu.be/dQw4w9WgXcQ"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.YouTubeQuotaExhausted);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Video_Is_Not_Found()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(It.IsAny<string>(), default))
            .ReturnsAsync((VideoCandidate?)null);

        var result = await CreateSut().ExecuteAsync(
            new GetVideoByUrlRequest("https://youtu.be/dQw4w9WgXcQ"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.VideoNotFound);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Video_Has_No_Captions()
    {
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Candidate());
        _transcriptProvider.Setup(x => x.HasEnglishCaptionsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        var result = await CreateSut().ExecuteAsync(
            new GetVideoByUrlRequest("https://youtu.be/dQw4w9WgXcQ"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.VideoNotEligible);
    }
}

using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoImport;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Application.UseCases.Shadowing.Video;
using Fluentra.Domain.Entities.Shadowing;
using Fluentra.Domain.Interfaces.Shadowing;
using Fluentra.Domain.ValueObjects.Shadowing;
using Fluentra.Shared.Messages;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Fluentra.Test.Application.UseCases.Shadowing.Video;

public sealed class ImportVideoUseCaseTests
{
    private const string VideoId = "dQw4w9WgXcQ";

    private readonly Mock<IVideoSearchProvider> _videoSearchProvider = new();
    private readonly Mock<IVideoTranscriptProvider> _transcriptProvider = new();
    private readonly Mock<IVideoRepository> _videoRepository = new();
    private readonly Mock<IYouTubeQuotaTracker> _quotaTracker = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public ImportVideoUseCaseTests()
    {
        _transcriptProvider.Setup(x => x.HasEnglishCaptionsAsync(It.IsAny<string>(), default)).ReturnsAsync(true);
    }

    private ImportVideoUseCase CreateSut() => new(
        _videoSearchProvider.Object, _transcriptProvider.Object, _videoRepository.Object,
        _quotaTracker.Object, _unitOfWork.Object, Options.Create(new YouTubeSettings()));

    private static VideoCandidate Candidate() =>
        new(VideoId, "Some Video", "https://example.com/thumb.jpg", TimeSpan.FromMinutes(5), 50_000, 1_000);

    private static List<TranscriptSegment> Segments() =>
    [
        new("First scene.", TimeSpan.Zero, TimeSpan.FromSeconds(3)),
        new("Second scene.", TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(6)),
    ];

    [Fact]
    public async Task Should_Return_Failure_When_Video_Id_Is_Invalid()
    {
        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest("not-valid"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.InvalidVideoUrl);
        _quotaTracker.Verify(x => x.TryConsumeAsync(It.IsAny<int>(), default), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Existing_Video_Without_Consuming_Quota()
    {
        var existing = new Domain.Entities.Shadowing.Video(
            new YouTubeVideoId(VideoId), "Already Imported", "thumb.jpg",
            new VideoDuration(TimeSpan.FromMinutes(5)), 10, 1);
        existing.AddScene("A scene.", new SceneTiming(TimeSpan.Zero, TimeSpan.FromSeconds(3)));

        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync(existing);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Title.ShouldBe("Already Imported");
        result.Value.Scenes.Count.ShouldBe(1);
        _quotaTracker.Verify(x => x.TryConsumeAsync(It.IsAny<int>(), default), Times.Never);
        _videoSearchProvider.Verify(x => x.GetByIdAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Quota_Is_Exhausted()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(false, true));

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.YouTubeQuotaExhausted);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Video_Is_Not_Found()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(VideoId, default)).ReturnsAsync((VideoCandidate?)null);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.VideoNotFound);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Video_Is_Not_Eligible()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(VideoId, default)).ReturnsAsync(Candidate());
        _transcriptProvider.Setup(x => x.HasEnglishCaptionsAsync(VideoId, default)).ReturnsAsync(false);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.VideoNotEligible);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Transcript_Is_Unavailable()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(VideoId, default)).ReturnsAsync(Candidate());
        _transcriptProvider.Setup(x => x.GetTranscriptAsync(VideoId, default)).ReturnsAsync([]);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.TranscriptUnavailable);
    }

    [Fact]
    public async Task Should_Import_And_Segment_Video_Into_Scenes()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(VideoId, default)).ReturnsAsync(Candidate());
        _transcriptProvider.Setup(x => x.GetTranscriptAsync(VideoId, default)).ReturnsAsync(Segments());
        _videoRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Shadowing.Video>())).ReturnsAsync(true);
        _unitOfWork.Setup(x => x.CommitAsync(default)).ReturnsAsync(true);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Title.ShouldBe("Some Video");
        result.Value.Scenes.Count.ShouldBe(2);
        result.Value.Scenes[0].Text.ShouldBe("First scene.");
        _videoRepository.Verify(x => x.AddAsync(It.Is<Domain.Entities.Shadowing.Video>(
            v => v.Scenes.Count == 2 && v.Scenes.First().Text == "First scene.")), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Commit_Fails()
    {
        _videoRepository.Setup(x => x.GetByYouTubeVideoIdAsync(VideoId)).ReturnsAsync((Domain.Entities.Shadowing.Video?)null);
        _quotaTracker.Setup(x => x.TryConsumeAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new YouTubeQuotaConsumptionResult(true, false));
        _videoSearchProvider.Setup(x => x.GetByIdAsync(VideoId, default)).ReturnsAsync(Candidate());
        _transcriptProvider.Setup(x => x.GetTranscriptAsync(VideoId, default)).ReturnsAsync(Segments());
        _videoRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Shadowing.Video>())).ReturnsAsync(true);
        _unitOfWork.Setup(x => x.CommitAsync(default)).ReturnsAsync(false);

        var result = await CreateSut().ExecuteAsync(new ImportVideoRequest(VideoId));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.PersistenceError);
    }
}

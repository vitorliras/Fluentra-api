using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;
using Fluentra.Application.UseCases.Shadowing.PronunciationEvaluation;
using Fluentra.Shared.Messages;
using Moq;
using Shouldly;

namespace Fluentra.Test.Application.UseCases.Shadowing.PronunciationEvaluation;

public sealed class EvaluatePronunciationUseCaseTests
{
    private readonly Mock<ISpeechTranscriber> _speechTranscriber = new();

    private EvaluatePronunciationUseCase CreateSut() => new(_speechTranscriber.Object);

    private static List<TranscribedWord> Words(params string[] texts) =>
        texts.Select((text, i) => new TranscribedWord(text, TimeSpan.FromSeconds(i), TimeSpan.FromSeconds(i + 1))).ToList();

    [Fact]
    public async Task Should_Return_Failure_When_No_Speech_Is_Detected()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default)).ReturnsAsync([]);

        var result = await CreateSut().ExecuteAsync(new EvaluatePronunciationRequest(Stream.Null, "let's set up the project"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(ShadowingErrorCodes.NoSpeechDetected);
    }

    [Fact]
    public async Task Should_Mark_All_Words_Correct_On_Exact_Match()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("Let's", "set", "up", "the", "project"));

        var result = await CreateSut().ExecuteAsync(
            new EvaluatePronunciationRequest(Stream.Null, "Let's set up the project"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Words.Count.ShouldBe(5);
        result.Value.Words.ShouldAllBe(w => w.Mark == "Correct");
        result.Value.AccuracyRate.ShouldBe(1.0);
        result.Value.ShouldRepeat.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Ignore_Case_And_Punctuation_When_Comparing()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("scratch"));

        var result = await CreateSut().ExecuteAsync(new EvaluatePronunciationRequest(Stream.Null, "scratch."));

        result.Value!.Words.Single().Mark.ShouldBe("Correct");
    }

    [Fact]
    public async Task Should_Mark_Similar_Word_As_Approximate()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("scratched"));

        var result = await CreateSut().ExecuteAsync(new EvaluatePronunciationRequest(Stream.Null, "scratch"));

        result.Value!.Words.Single().Mark.ShouldBe("Approximate");
        result.Value.AccuracyRate.ShouldBe(0.5);
        result.Value.ShouldRepeat.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Mark_Very_Different_Word_As_Incorrect()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("banana"));

        var result = await CreateSut().ExecuteAsync(new EvaluatePronunciationRequest(Stream.Null, "scratch"));

        result.Value!.Words.Single().Mark.ShouldBe("Incorrect");
        result.Value.AccuracyRate.ShouldBe(0.0);
        result.Value.ShouldRepeat.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Mark_Missing_Word_As_Incorrect_With_No_Recognized_Word()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("let's", "up", "the", "project"));

        var result = await CreateSut().ExecuteAsync(
            new EvaluatePronunciationRequest(Stream.Null, "let's set up the project"));

        result.Value!.Words.Count.ShouldBe(5);
        var missing = result.Value.Words.Single(w => w.TargetWord == "set");
        missing.Mark.ShouldBe("Incorrect");
        missing.RecognizedWord.ShouldBeNull();
        result.Value.AccuracyRate.ShouldBe(0.8);
        result.Value.ShouldRepeat.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Ignore_Extra_Spoken_Word_Without_Breaking_Alignment()
    {
        _speechTranscriber.Setup(x => x.TranscribeAsync(It.IsAny<Stream>(), default))
            .ReturnsAsync(Words("let's", "really", "set", "up", "the", "project"));

        var result = await CreateSut().ExecuteAsync(
            new EvaluatePronunciationRequest(Stream.Null, "let's set up the project"));

        result.Value!.Words.Count.ShouldBe(5);
        result.Value.Words.ShouldAllBe(w => w.Mark == "Correct");
        result.Value.AccuracyRate.ShouldBe(1.0);
    }
}

using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;

namespace Fluentra.Application.UseCases.Shadowing.PronunciationEvaluation;

public sealed class EvaluatePronunciationUseCase : IUseCase<EvaluatePronunciationRequest, EvaluatePronunciationResponse>
{
    private const string CorrectMark = "Correct";
    private const string ApproximateMark = "Approximate";
    private const string IncorrectMark = "Incorrect";

    private const double ApproximateSimilarityThreshold = 0.5;
    private const double ApproximatePartialCredit = 0.5;
    private const double RepeatThreshold = 0.55;

    private static readonly char[] TrimChars = ['.', ',', '!', '?', ';', ':', '"', '\'', '(', ')'];

    private readonly ISpeechTranscriber _speechTranscriber;

    public EvaluatePronunciationUseCase(ISpeechTranscriber speechTranscriber)
    {
        _speechTranscriber = speechTranscriber;
    }

    public async Task<Result<EvaluatePronunciationResponse>> ExecuteAsync(
        EvaluatePronunciationRequest request,
        CancellationToken cancellationToken = default)
    {
        var transcribed = await _speechTranscriber.TranscribeAsync(request.AudioWav, cancellationToken);
        if (transcribed.Count == 0)
            return Result<EvaluatePronunciationResponse>.Failure(Error.From(ShadowingErrorCodes.NoSpeechDetected));

        var targetWords = SplitWords(request.TargetText);
        var recognizedWords = transcribed.Select(x => x.Text).ToList();

        var alignment = AlignWords(
            targetWords.Select(Normalize).ToList(),
            recognizedWords.Select(Normalize).ToList());

        var evaluations = new List<WordEvaluation>();
        var creditedScore = 0.0;

        foreach (var (targetIndex, recognizedIndex) in alignment)
        {
            if (targetIndex is null)
                continue;

            var targetWord = targetWords[targetIndex.Value];
            var recognizedWord = recognizedIndex is not null ? recognizedWords[recognizedIndex.Value] : null;

            var similarity = recognizedWord is null
                ? 0.0
                : SimilarityRatio(Normalize(targetWord), Normalize(recognizedWord));

            var mark = similarity switch
            {
                >= 1.0 => CorrectMark,
                >= ApproximateSimilarityThreshold => ApproximateMark,
                _ => IncorrectMark,
            };

            creditedScore += mark switch
            {
                CorrectMark => 1.0,
                ApproximateMark => ApproximatePartialCredit,
                _ => 0.0,
            };

            evaluations.Add(new WordEvaluation(targetWord, recognizedWord, mark));
        }

        var accuracyRate = evaluations.Count == 0 ? 0.0 : creditedScore / evaluations.Count;
        var shouldRepeat = accuracyRate < RepeatThreshold;

        return Result<EvaluatePronunciationResponse>.Success(
            new EvaluatePronunciationResponse(evaluations, accuracyRate, shouldRepeat));
    }

    private static List<string> SplitWords(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static string Normalize(string word) => word.Trim().Trim(TrimChars).ToLowerInvariant();

    private static double SimilarityRatio(string a, string b)
    {
        if (a == b)
            return 1.0;

        var maxLength = Math.Max(a.Length, b.Length);
        if (maxLength == 0)
            return 1.0;

        var distance = LevenshteinDistance(a, b);
        return 1.0 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var costs = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
            costs[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            costs[0] = i;
            var previousDiagonal = i - 1;

            for (var j = 1; j <= b.Length; j++)
            {
                var previousCosts = costs[j];
                var substitutionCost = a[i - 1] == b[j - 1] ? previousDiagonal : previousDiagonal + 1;
                costs[j] = Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), substitutionCost);
                previousDiagonal = previousCosts;
            }
        }

        return costs[b.Length];
    }

    private static List<(int? TargetIndex, int? RecognizedIndex)> AlignWords(
        IReadOnlyList<string> target, IReadOnlyList<string> recognized)
    {
        var targetCount = target.Count;
        var recognizedCount = recognized.Count;
        var cost = new int[targetCount + 1, recognizedCount + 1];

        for (var i = 0; i <= targetCount; i++)
            cost[i, 0] = i;
        for (var j = 0; j <= recognizedCount; j++)
            cost[0, j] = j;

        for (var i = 1; i <= targetCount; i++)
        {
            for (var j = 1; j <= recognizedCount; j++)
            {
                var substitutionCost = target[i - 1] == recognized[j - 1] ? 0 : 1;
                cost[i, j] = Math.Min(
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1),
                    cost[i - 1, j - 1] + substitutionCost);
            }
        }

        var alignment = new List<(int? TargetIndex, int? RecognizedIndex)>();
        var row = targetCount;
        var col = recognizedCount;

        while (row > 0 || col > 0)
        {
            var matchStep = row > 0 && col > 0 &&
                cost[row, col] == cost[row - 1, col - 1] + (target[row - 1] == recognized[col - 1] ? 0 : 1);

            if (matchStep)
            {
                alignment.Add((row - 1, col - 1));
                row--;
                col--;
            }
            else if (row > 0 && cost[row, col] == cost[row - 1, col] + 1)
            {
                alignment.Add((row - 1, null));
                row--;
            }
            else
            {
                alignment.Add((null, col - 1));
                col--;
            }
        }

        alignment.Reverse();
        return alignment;
    }
}

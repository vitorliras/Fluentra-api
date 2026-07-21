namespace Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;

public sealed record EvaluatePronunciationResponse(IReadOnlyList<WordEvaluation> Words, double AccuracyRate);

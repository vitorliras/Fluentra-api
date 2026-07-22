namespace Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;

public sealed record EvaluatePronunciationRequest(Stream AudioWav, string TargetText, int SceneId);

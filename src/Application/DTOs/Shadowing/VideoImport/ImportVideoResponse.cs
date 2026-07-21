namespace Fluentra.Application.DTOs.Shadowing.VideoImport;

public sealed record ImportVideoResponse(int VideoId, string Title, IReadOnlyList<SceneDto> Scenes);

public sealed record SceneDto(int Id, string Text, string? Translation, double StartSeconds, double EndSeconds, int SequenceOrder);

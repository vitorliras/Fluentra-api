using Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;
using Fluentra.Application.Executors;
using Fluentra.Application.Pipelines;
using Fluentra.Application.UseCases.Shadowing.PronunciationEvaluation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fluentra.Api.Controllers;

public sealed class EvaluatePronunciationHttpRequest
{
    public IFormFile Audio { get; set; } = null!;
    public string TargetText { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("shadowing/pronunciation")]
public sealed class ShadowingPronunciationController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly EvaluatePronunciationUseCase _evaluatePronunciation;

    public ShadowingPronunciationController(
        UseCaseExecutor executor,
        EvaluatePronunciationUseCase evaluatePronunciation)
    {
        _executor = executor;
        _evaluatePronunciation = evaluatePronunciation;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate(
        [FromForm] EvaluatePronunciationHttpRequest httpRequest,
        [FromServices] ValidationPipeline<EvaluatePronunciationRequest, EvaluatePronunciationResponse> pipeline)
    {
        await using var audioStream = httpRequest.Audio.OpenReadStream();
        var request = new EvaluatePronunciationRequest(audioStream, httpRequest.TargetText);

        var result = await _executor.ExecuteAsync(request, _evaluatePronunciation, pipeline);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}

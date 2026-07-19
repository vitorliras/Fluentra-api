using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Fluentra.Application.Executors;
using Fluentra.Application.Pipelines;
using Fluentra.Application.UseCases.Shadowing.Video;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fluentra.Api.Controllers;

[Authorize]
[ApiController]
[Route("shadowing/videos")]
public sealed class ShadowingVideosController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly SearchVideosUseCase _searchVideos;

    public ShadowingVideosController(
        UseCaseExecutor executor,
        SearchVideosUseCase searchVideos)
    {
        _executor = executor;
        _searchVideos = searchVideos;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        SearchVideosRequest request,
        [FromServices] ValidationPipeline<SearchVideosRequest, SearchVideosResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _searchVideos, pipeline);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}

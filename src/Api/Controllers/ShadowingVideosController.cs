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
    private readonly GetVideoByUrlUseCase _getVideoByUrl;

    public ShadowingVideosController(
        UseCaseExecutor executor,
        SearchVideosUseCase searchVideos,
        GetVideoByUrlUseCase getVideoByUrl)
    {
        _executor = executor;
        _searchVideos = searchVideos;
        _getVideoByUrl = getVideoByUrl;
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

    [HttpPost("by-url")]
    public async Task<IActionResult> GetByUrl(
        GetVideoByUrlRequest request,
        [FromServices] ValidationPipeline<GetVideoByUrlRequest, VideoSearchResultItem> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _getVideoByUrl, pipeline);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}

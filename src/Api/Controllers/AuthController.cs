using Fluentra.Application.DTOs.Auth.Session;
using Fluentra.Application.Executors;
using Fluentra.Application.Pipelines;
using Fluentra.Application.UseCases.Auth.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fluentra.Api.Controllers;

[Authorize]
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly LoginUseCase _login;

    public AuthController(
        UseCaseExecutor executor,
        LoginUseCase login)
    {
        _executor = executor;
        _login = login;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        [FromServices] ValidationPipeline<LoginRequest, LoginResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _login, pipeline);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}

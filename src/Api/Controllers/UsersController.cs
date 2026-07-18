using Fluentra.Application.DTOs.Users.User;
using Fluentra.Application.Executors;
using Fluentra.Application.Pipelines;
using Fluentra.Application.UseCases.Users.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fluentra.Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateUserUseCase _createUser;

    public UsersController(
        UseCaseExecutor executor,
        CreateUserUseCase createUser)
    {
        _executor = executor;
        _createUser = createUser;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateUserRequest request,
        [FromServices] ValidationPipeline<CreateUserRequest, UserResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _createUser, pipeline);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}

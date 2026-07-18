using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Auth.Session;
using Fluentra.Domain.Interfaces.Users;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;

namespace Fluentra.Application.UseCases.Auth.Session;

public sealed class LoginUseCase : IUseCase<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = request.Identifier.Contains('@')
            ? await _userRepository.GetByEmailAsync(request.Identifier)
            : await _userRepository.GetByUsernameAsync(request.Identifier);

        if (user is null)
            return Result<LoginResponse>.Failure(Error.From(AuthErrorCodes.InvalidCredentials));

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure(Error.From(AuthErrorCodes.InvalidCredentials));

        var token = _tokenService.GenerateToken(user);

        return Result<LoginResponse>.Success(
            new LoginResponse(token.Token, token.ExpiresAt, user.Name.Value, user.Username.Value));
    }
}

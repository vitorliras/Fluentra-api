using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Auth.Session;
using Fluentra.Application.UseCases.Auth.Session;
using Fluentra.Domain.Interfaces.Users;
using Fluentra.Domain.ValueObjects.User;
using Fluentra.Shared.Messages;
using Moq;
using Shouldly;
using DomainUser = Fluentra.Domain.Entities.Users.User;

namespace Fluentra.Test.Application.UseCases.Auth.Session;

public sealed class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private LoginUseCase CreateSut() =>
        new(_userRepository.Object, _passwordHasher.Object, _tokenService.Object);

    private static DomainUser ExistingUser() => new(
        new Name("Vitor Lira"),
        new Username("vitorlira"),
        new Email("vitor@example.com"),
        "hashed-password");

    [Fact]
    public async Task Should_Login_When_Credentials_Are_Valid_With_Email()
    {
        _userRepository.Setup(x => x.GetByEmailAsync("vitor@example.com")).ReturnsAsync(ExistingUser());
        _passwordHasher.Setup(x => x.Verify("Senha123!", "hashed-password")).Returns(true);
        _tokenService.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
            .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(4)));

        var result = await CreateSut().ExecuteAsync(new LoginRequest("vitor@example.com", "Senha123!"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.AccessToken.ShouldBe("jwt-token");
    }

    [Fact]
    public async Task Should_Login_When_Credentials_Are_Valid_With_Username()
    {
        _userRepository.Setup(x => x.GetByUsernameAsync("vitorlira")).ReturnsAsync(ExistingUser());
        _passwordHasher.Setup(x => x.Verify("Senha123!", "hashed-password")).Returns(true);
        _tokenService.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
            .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(4)));

        var result = await CreateSut().ExecuteAsync(new LoginRequest("vitorlira", "Senha123!"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Username.ShouldBe("vitorlira");
    }

    [Fact]
    public async Task Should_Return_Failure_When_User_Not_Found()
    {
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((DomainUser?)null);

        var result = await CreateSut().ExecuteAsync(new LoginRequest("naoexiste@example.com", "Senha123!"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(AuthErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Password_Is_Incorrect()
    {
        _userRepository.Setup(x => x.GetByEmailAsync("vitor@example.com")).ReturnsAsync(ExistingUser());
        _passwordHasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await CreateSut().ExecuteAsync(new LoginRequest("vitor@example.com", "SenhaErrada1!"));

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(AuthErrorCodes.InvalidCredentials);
    }
}

using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Users.User;
using Fluentra.Application.UseCases.Users.User;
using Fluentra.Domain.Interfaces.Users;
using Fluentra.Shared.Messages;
using Moq;
using Shouldly;
using DomainUser = Fluentra.Domain.Entities.Users.User;

namespace Fluentra.Test.Application.UseCases.Users.User;

public sealed class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateUserUseCase CreateSut() =>
        new(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object);

    private static CreateUserRequest ValidRequest() =>
        new("Vitor Lira", "vitorlira", "vitor@example.com", "Senha123!");

    [Fact]
    public async Task Should_Create_User_When_Request_Is_Valid()
    {
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((DomainUser?)null);
        _userRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((DomainUser?)null);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");
        _userRepository.Setup(x => x.AddAsync(It.IsAny<DomainUser>())).ReturnsAsync(true);
        _unitOfWork.Setup(x => x.CommitAsync(default)).ReturnsAsync(true);

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Username.ShouldBe("vitorlira");
    }

    [Fact]
    public async Task Should_Return_Failure_When_Email_Already_Exists()
    {
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new DomainUser(
                new Fluentra.Domain.ValueObjects.User.Name("Outro Usuário"),
                new Fluentra.Domain.ValueObjects.User.Username("outrouser"),
                new Fluentra.Domain.ValueObjects.User.Email("vitor@example.com"),
                "hash"));

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(UsersErrorCodes.EmailAlreadyExists);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Username_Already_Exists()
    {
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((DomainUser?)null);
        _userRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(new DomainUser(
                new Fluentra.Domain.ValueObjects.User.Name("Outro Usuário"),
                new Fluentra.Domain.ValueObjects.User.Username("vitorlira"),
                new Fluentra.Domain.ValueObjects.User.Email("outro@example.com"),
                "hash"));

        var result = await CreateSut().ExecuteAsync(ValidRequest());

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(UsersErrorCodes.UsernameAlreadyExists);
    }
}

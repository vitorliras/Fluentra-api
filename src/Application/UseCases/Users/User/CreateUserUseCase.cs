using Fluentra.Application.Abstractions;
using Fluentra.Application.DTOs.Users.User;
using Fluentra.Domain.Interfaces.Users;
using Fluentra.Shared.Messages;
using Fluentra.Shared.Results;
using DomainUser = Fluentra.Domain.Entities.Users.User;
using EmailVo = Fluentra.Domain.ValueObjects.User.Email;
using NameVo = Fluentra.Domain.ValueObjects.User.Name;
using UsernameVo = Fluentra.Domain.ValueObjects.User.Username;

namespace Fluentra.Application.UseCases.Users.User;

public sealed class CreateUserUseCase : IUseCase<CreateUserRequest, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> ExecuteAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = new EmailVo(request.Email);
        var username = new UsernameVo(request.Username);

        if (await _userRepository.GetByEmailAsync(email.Value) is not null)
            return Result<UserResponse>.Failure(Error.From(UsersErrorCodes.EmailAlreadyExists));

        if (await _userRepository.GetByUsernameAsync(username.Value) is not null)
            return Result<UserResponse>.Failure(Error.From(UsersErrorCodes.UsernameAlreadyExists));

        var name = new NameVo(request.Name);
        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new DomainUser(name, username, email, passwordHash);

        var added = await _userRepository.AddAsync(user);
        if (!added)
            return Result<UserResponse>.Failure(Error.From(UsersErrorCodes.InsertFailed));

        var committed = await _unitOfWork.CommitAsync(cancellationToken);
        if (!committed)
            return Result<UserResponse>.Failure(Error.From(UsersErrorCodes.PersistenceError));

        return Result<UserResponse>.Success(
            new UserResponse(user.Id, user.Name.Value, user.Username.Value, user.Email.Value));
    }
}

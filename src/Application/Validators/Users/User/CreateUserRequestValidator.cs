using Fluentra.Application.DTOs.Users.User;
using FluentValidation;

namespace Fluentra.Application.Validators.Users.User;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(30)
            .Must(value => !value.Contains(' '))
            .WithMessage("UsernameCannotContainWhitespace");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"\d")
            .WithMessage("PasswordMustContainDigit")
            .Matches(@"[^\w\s]")
            .WithMessage("PasswordMustContainSpecialCharacter");
    }
}

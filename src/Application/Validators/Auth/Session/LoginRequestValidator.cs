using Fluentra.Application.DTOs.Auth.Session;
using FluentValidation;

namespace Fluentra.Application.Validators.Auth.Session;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

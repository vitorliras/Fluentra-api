using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using FluentValidation;

namespace Fluentra.Application.Validators.Shadowing.Video;

public sealed class SearchVideosRequestValidator : AbstractValidator<SearchVideosRequest>
{
    public SearchVideosRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DesiredDurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(180);
    }
}

using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using FluentValidation;

namespace Fluentra.Application.Validators.Shadowing.Video;

public sealed class GetVideoByUrlRequestValidator : AbstractValidator<GetVideoByUrlRequest>
{
    public GetVideoByUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(500);
    }
}

using Fluentra.Application.DTOs.Shadowing.VideoImport;
using FluentValidation;

namespace Fluentra.Application.Validators.Shadowing.Video;

public sealed class ImportVideoRequestValidator : AbstractValidator<ImportVideoRequest>
{
    public ImportVideoRequestValidator()
    {
        RuleFor(x => x.YouTubeVideoId)
            .NotEmpty()
            .Length(11);
    }
}

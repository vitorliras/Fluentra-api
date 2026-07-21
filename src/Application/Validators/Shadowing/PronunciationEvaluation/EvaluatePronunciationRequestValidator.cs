using Fluentra.Application.DTOs.Shadowing.PronunciationEvaluation;
using FluentValidation;

namespace Fluentra.Application.Validators.Shadowing.PronunciationEvaluation;

public sealed class EvaluatePronunciationRequestValidator : AbstractValidator<EvaluatePronunciationRequest>
{
    public EvaluatePronunciationRequestValidator()
    {
        RuleFor(x => x.TargetText)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.AudioWav)
            .NotNull();
    }
}

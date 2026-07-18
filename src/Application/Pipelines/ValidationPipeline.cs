using Fluentra.Application.Abstractions;
using Fluentra.Shared.Results;
using FluentValidation;

namespace Fluentra.Application.Pipelines;

public sealed class ValidationPipeline<TRequest, TResponse>
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationPipeline(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<Result<TResponse>> HandleAsync(
        TRequest request,
        IUseCase<TRequest, TResponse> useCase,
        CancellationToken cancellationToken = default)
    {
        if (_validator is not null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return Result<TResponse>.Failure(Error.From(validationResult.Errors.First().ErrorCode));
        }

        return await useCase.ExecuteAsync(request, cancellationToken);
    }
}

using Fluentra.Application.Abstractions;
using Fluentra.Application.Pipelines;
using Fluentra.Shared.Results;

namespace Fluentra.Application.Executors;

public sealed class UseCaseExecutor
{
    public async Task<Result<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        IUseCase<TRequest, TResponse> useCase,
        ValidationPipeline<TRequest, TResponse> pipeline)
    {
        return await pipeline.HandleAsync(request, useCase);
    }

    public async Task<Result<TResponse>> ExecuteAsync<TResponse>(IUseCaseWithoutRequest<TResponse> useCase)
    {
        return await useCase.ExecuteAsync();
    }
}

using Fluentra.Shared.Results;

namespace Fluentra.Application.Abstractions;

public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

using Fluentra.Shared.Results;

namespace Fluentra.Application.Abstractions;

public interface IUseCaseWithoutRequest<TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(CancellationToken cancellationToken = default);
}

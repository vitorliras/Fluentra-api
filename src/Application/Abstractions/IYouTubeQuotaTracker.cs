namespace Fluentra.Application.Abstractions;

public interface IYouTubeQuotaTracker
{
    Task<YouTubeQuotaConsumptionResult> TryConsumeAsync(int units, CancellationToken cancellationToken = default);
}

public readonly record struct YouTubeQuotaConsumptionResult(bool Allowed, bool NearLimit);

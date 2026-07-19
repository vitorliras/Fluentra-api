using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

// commits directly - consumption must persist even if the rest of the request fails
public sealed class YouTubeQuotaTracker : IYouTubeQuotaTracker
{
    private const double NearLimitThreshold = 0.8;

    private readonly AppDbContext _context;
    private readonly YouTubeSettings _settings;

    public YouTubeQuotaTracker(AppDbContext context, IOptions<YouTubeSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<YouTubeQuotaConsumptionResult> TryConsumeAsync(int units, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var usage = await _context.Set<YouTubeQuotaUsage>()
            .FirstOrDefaultAsync(x => x.Date == today, cancellationToken);

        if (usage is null)
        {
            usage = new YouTubeQuotaUsage { Date = today, UnitsConsumed = 0 };
            await _context.Set<YouTubeQuotaUsage>().AddAsync(usage, cancellationToken);
        }

        if (usage.UnitsConsumed + units > _settings.DailyQuotaLimit)
            return new YouTubeQuotaConsumptionResult(false, true);

        usage.UnitsConsumed += units;
        await _context.SaveChangesAsync(cancellationToken);

        var nearLimit = usage.UnitsConsumed >= _settings.DailyQuotaLimit * NearLimitThreshold;
        return new YouTubeQuotaConsumptionResult(true, nearLimit);
    }
}

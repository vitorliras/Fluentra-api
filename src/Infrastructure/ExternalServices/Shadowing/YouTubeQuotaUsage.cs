namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed class YouTubeQuotaUsage
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int UnitsConsumed { get; set; }
}

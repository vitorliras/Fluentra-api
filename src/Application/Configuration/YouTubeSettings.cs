namespace Fluentra.Application.Configuration;

public sealed class YouTubeSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://www.googleapis.com/youtube/v3/";
    public int DailyQuotaLimit { get; init; } = 10_000;
    public int SearchCostUnits { get; init; } = 100;
}

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Fluentra.Application.Abstractions;
using Fluentra.Application.Configuration;
using Fluentra.Application.DTOs.Shadowing.VideoSearch;
using Microsoft.Extensions.Options;

namespace Fluentra.Infrastructure.ExternalServices.Shadowing;

public sealed class YouTubeVideoSearchProvider : IVideoSearchProvider
{
    private const int MaxSearchResults = 25;

    private readonly HttpClient _httpClient;
    private readonly YouTubeSettings _settings;

    public YouTubeVideoSearchProvider(HttpClient httpClient, IOptions<YouTubeSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyList<VideoCandidate>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchUrl = $"search?part=snippet&type=video&maxResults={MaxSearchResults}" +
                         $"&q={Uri.EscapeDataString(query)}&key={_settings.ApiKey}";

        var searchResponse = await _httpClient.GetFromJsonAsync<YouTubeSearchResponse>(searchUrl, cancellationToken);

        var videoIds = (searchResponse?.Items ?? [])
            .Select(item => item.Id.VideoId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        if (videoIds.Count == 0)
            return [];

        var detailsUrl = $"videos?part=snippet,contentDetails,statistics" +
                          $"&id={string.Join(',', videoIds)}&key={_settings.ApiKey}";

        var detailsResponse = await _httpClient.GetFromJsonAsync<YouTubeVideosResponse>(detailsUrl, cancellationToken);

        return (detailsResponse?.Items ?? []).Select(ToVideoCandidate).ToList();
    }

    public async Task<VideoCandidate?> GetByIdAsync(string youTubeVideoId, CancellationToken cancellationToken = default)
    {
        var detailsUrl = $"videos?part=snippet,contentDetails,statistics" +
                          $"&id={Uri.EscapeDataString(youTubeVideoId)}&key={_settings.ApiKey}";

        var detailsResponse = await _httpClient.GetFromJsonAsync<YouTubeVideosResponse>(detailsUrl, cancellationToken);
        var item = detailsResponse?.Items?.FirstOrDefault();

        return item is null ? null : ToVideoCandidate(item);
    }

    private static VideoCandidate ToVideoCandidate(YouTubeVideoItem item)
    {
        return new VideoCandidate(
            item.Id,
            item.Snippet.Title,
            item.Snippet.Thumbnails.Medium?.Url ?? item.Snippet.Thumbnails.Default?.Url ?? string.Empty,
            XmlConvert.ToTimeSpan(item.ContentDetails.Duration),
            ParseCount(item.Statistics.ViewCount),
            ParseCount(item.Statistics.LikeCount));
    }

    private static long ParseCount(string? value) => long.TryParse(value, out var parsed) ? parsed : 0;

    private sealed class YouTubeSearchResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeSearchItem> Items { get; set; } = [];
    }

    private sealed class YouTubeSearchItem
    {
        [JsonPropertyName("id")]
        public YouTubeSearchItemId Id { get; set; } = new();
    }

    private sealed class YouTubeSearchItemId
    {
        [JsonPropertyName("videoId")]
        public string? VideoId { get; set; }
    }

    private sealed class YouTubeVideosResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeVideoItem> Items { get; set; } = [];
    }

    private sealed class YouTubeVideoItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public YouTubeVideoSnippet Snippet { get; set; } = new();

        [JsonPropertyName("contentDetails")]
        public YouTubeVideoContentDetails ContentDetails { get; set; } = new();

        [JsonPropertyName("statistics")]
        public YouTubeVideoStatistics Statistics { get; set; } = new();
    }

    private sealed class YouTubeVideoSnippet
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("thumbnails")]
        public YouTubeVideoThumbnails Thumbnails { get; set; } = new();
    }

    private sealed class YouTubeVideoThumbnails
    {
        [JsonPropertyName("default")]
        public YouTubeVideoThumbnail? Default { get; set; }

        [JsonPropertyName("medium")]
        public YouTubeVideoThumbnail? Medium { get; set; }
    }

    private sealed class YouTubeVideoThumbnail
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    private sealed class YouTubeVideoContentDetails
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; } = "PT0S";
    }

    private sealed class YouTubeVideoStatistics
    {
        [JsonPropertyName("viewCount")]
        public string? ViewCount { get; set; }

        [JsonPropertyName("likeCount")]
        public string? LikeCount { get; set; }
    }
}

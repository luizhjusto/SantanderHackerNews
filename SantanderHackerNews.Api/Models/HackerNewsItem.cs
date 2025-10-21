using System.Text.Json.Serialization;

namespace Santander.HackerNews.Api.Models;

/// <summary>
/// Partial model representing Hacker News item JSON
/// See: https://github.com/HackerNews/API
/// </summary>
public class HackerNewsItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    // Hacker News uses "url" for external link; "text" for internal posts
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("by")]
    public string? By { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; } // unix epoch seconds

    [JsonPropertyName("score")]
    public int? Score { get; set; }

    [JsonPropertyName("descendants")]
    public int? Descendants { get; set; } // comment count
}

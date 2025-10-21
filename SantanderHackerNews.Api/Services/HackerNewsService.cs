using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Santander.HackerNews.Api.Configuration;
using Santander.HackerNews.Api.Models;

namespace Santander.HackerNews.Api.Services;

public class HackerNewsService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<HackerNewsService> logger) : IHackerNewsService
{
    private static readonly TimeSpan BestStoriesCacheDuration = TimeSpan.FromMinutes(Settings.HackerNews.CacheDurations.BestStoriesMinutes);
    private static readonly TimeSpan ItemCacheDuration = TimeSpan.FromMinutes(Settings.HackerNews.CacheDurations.ItemMinutes);
    private readonly int MaxConcurrentRequests = Settings.HackerNews.MaxConcurrentRequests;

    public async Task<IEnumerable<StoryDto?>?> GetFirstNBestStoriesAsync(int n, CancellationToken cancellationToken = default)
    {
        var ids = await GetBestStoriesIdsAsync(cancellationToken);

        if (ids.Length == 0)
            return [];

        var takeIds = ids.Take(n).ToArray();
        var semaphore = new SemaphoreSlim(MaxConcurrentRequests);
        var tasks = takeIds?.Select(async id =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await GetItemAsync(id, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        var items = tasks != null ? await Task.WhenAll(tasks) : [];

        var dtos = items
            .Where(i => i != null)
            .Select(MapToDto)
            .OrderByDescending(s => s?.Score)
            .ToArray();

        return dtos;
    }

    #region Private methods

    private async Task<int[]> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "beststories_ids";

        if (cache.TryGetValue<int[]>(cacheKey, out int[]? cached))
            return cached ?? [];

        var client = httpClientFactory.CreateClient("hackernews");

        var response = await client.GetAsync("beststories.json", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var ids = await JsonSerializer.DeserializeAsync<int[]>(stream, cancellationToken: cancellationToken) ?? [];

        cache.Set(cacheKey, ids, BestStoriesCacheDuration);

        return ids;
    }

    private async Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken)
    {
        string cacheKey = $"item_{id}";

        if (cache.TryGetValue<HackerNewsItem>(cacheKey, out var cachedItem))
            return cachedItem;

        var client = httpClientFactory.CreateClient("hackernews");
        var response = await client.GetAsync($"item/{id}.json", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to fetch item {Id}. Status: {StatusCode}", id, response.StatusCode);
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var item = await JsonSerializer.DeserializeAsync<HackerNewsItem>(stream, cancellationToken: cancellationToken);

        if (item != null)
        {
            cache.Set(cacheKey, item, ItemCacheDuration);
        }

        return item;
    }

    private static StoryDto? MapToDto(HackerNewsItem? item)
    {
        if (item is null)
            return null;

        return new()
        {
            Title = item.Title ?? string.Empty,
            Uri = item.Url,
            PostedBy = item.By,
            Time = DateTimeOffset.FromUnixTimeSeconds(item.Time).ToString("yyyy-MM-ddTHH:mm:ss+00:00"),
            Score = item.Score ?? 0,
            CommentCount = item.Descendants ?? 0
        };
    }

    #endregion
}
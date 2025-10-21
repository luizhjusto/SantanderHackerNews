using Santander.HackerNews.Api.Models;

namespace Santander.HackerNews.Api.Services;

public interface IHackerNewsService
{
    /// <summary>
    /// Returns the details for the first n IDs from /v0/beststories.json
    /// (i.e., take first n IDs, fetch them, then sort by score descending).
    /// </summary>
    Task<IEnumerable<StoryDto?>?> GetFirstNBestStoriesAsync(int n, CancellationToken cancellationToken = default);
}
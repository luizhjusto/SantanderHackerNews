using Microsoft.AspNetCore.Mvc;
using Santander.HackerNews.Api.Services;

namespace Santander.HackerNews.Api.Controllers;

[ApiController]
[Route("api/v1/stories")]
public class StoriesController(IHackerNewsService hnService, ILogger<StoriesController> logger) : ControllerBase
{

    /// <summary>
    /// Returns the first n best stories (as returned by Hacker News) sorted by score descending.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            return BadRequest("count must be greater than 0");

        // Limit count to avoid abuse (server-side safety)
        const int MaxCount = 200;
        if (count > MaxCount)
            count = MaxCount;

        try
        {
            var stories = await hnService.GetFirstNBestStoriesAsync(count, cancellationToken);

            return Ok(stories);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Request cancelled.");
            return StatusCode(499); // client closed request (non-standard)
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching stories");
            return StatusCode(500, "Unexpected error");
        }
    }
}
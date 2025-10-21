namespace Santander.HackerNews.Api.Models;

public class StoryDto
{
    public string Title { get; set; } = string.Empty;
    public string? Uri { get; set; }
    public string? PostedBy { get; set; }
    public string Time { get; set; } = string.Empty; // ISO string like 2019-10-12T13:43:01+00:00
    public int Score { get; set; }
    public int CommentCount { get; set; }
}
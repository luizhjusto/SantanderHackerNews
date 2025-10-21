namespace Santander.HackerNews.Api.Configuration
{
    public static class Settings
    {
        public static HackerNewsSettings HackerNews { get; set; } = new();

        public class HackerNewsSettings
        {
            public string BaseUrl { get; set; } = string.Empty;
            public CacheDurationsSettings CacheDurations { get; set; } = new();
            public int MaxConcurrentRequests { get; set; }
        }

        public class CacheDurationsSettings
        {
            public int BestStoriesMinutes { get; set; }
            public int ItemMinutes { get; set; }
        }
    }
}
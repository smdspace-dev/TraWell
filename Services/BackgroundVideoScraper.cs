namespace TraWell.Services;

public class BackgroundVideoScraper : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BackgroundVideoScraper> _logger;

    public BackgroundVideoScraper(
        IServiceProvider services,
        ILogger<BackgroundVideoScraper> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var videoService = scope.ServiceProvider.GetRequiredService<VideoService>();
                    await videoService.ScrapeAndUpdateVideos();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scraping videos");
            }

            // Wait for 5 days before next scrape
            await Task.Delay(TimeSpan.FromDays(5), stoppingToken);
        }
    }
}

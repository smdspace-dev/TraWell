using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;
using YoutubeExplode.Search;
using TraWell.Models;

namespace TraWell.Services;

public class VideoService
{
    private readonly YoutubeClient _youtube;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    public VideoService(IConfiguration configuration, IServiceProvider services)
    {
        _youtube = new YoutubeClient();
        _configuration = configuration;
        _services = services;
    }

    public async Task<List<Video>> GetVideosForPlace(string placeName)
    {
        var searchQuery = $"{placeName} travel guide india";
        var searchResults = _youtube.Search.GetVideosAsync(searchQuery);
        var videos = new List<VideoSearchResult>();
        await foreach (var video in searchResults)
        {
            videos.Add(video);
            if (videos.Count >= 5) break;
        }

        return videos.Select(v => new Video
        {
            Id = v.Id.Value,
            Title = v.Title,
            Description = v.Title, // VideoSearchResult doesn't include full description
            ThumbnailUrl = v.Thumbnails.FirstOrDefault()?.Url ?? "",
            PublishedAt = DateTime.UtcNow // VideoSearchResult doesn't include publish date
        }).ToList();
    }

    public async Task ScrapeAndUpdateVideos()
    {
        // Get all places from the database using PlaceService
        using var scope = _services.CreateScope();
            
        var placeService = scope.ServiceProvider.GetRequiredService<PlaceService>();
        var places = await placeService.GetAllPlaces();

        foreach (var place in places)
        {
            try
            {
                // Get fresh videos for each place
                var videos = await GetVideosForPlace(place.Name);

                // TODO: Store videos in the database
                // For now, just wait a bit to avoid hitting YouTube API limits
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                // Log the error but continue with other places
                Console.Error.WriteLine($"Error updating videos for {place.Name}: {ex.Message}");
            }
        }
    }
}

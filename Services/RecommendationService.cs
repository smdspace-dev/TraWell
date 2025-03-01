using Microsoft.ML;
using TraWell.Models;

namespace TraWell.Services;

public class RecommendationService
{
    private readonly PlaceService _placeService;
    private readonly MLContext _mlContext;

    public RecommendationService(PlaceService placeService)
    {
        _placeService = placeService;
        _mlContext = new MLContext();
    }

    public async Task<List<Place>> GetRecommendations(UserPreferences preferences)
    {
        var places = await _placeService.GetAllPlaces();
        
        // Filter places based on user preferences
        var filteredPlaces = places.Where(place =>
            place.Categories.Intersect(preferences.Activities).Any() &&
            place.BestSeasons.Contains(preferences.Season) &&
            place.RecommendedDuration <= preferences.Duration
        ).ToList();

        // Sort places by rating
        return filteredPlaces.OrderByDescending(p => p.Rating).Take(10).ToList();
    }
}

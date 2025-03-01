using Microsoft.Extensions.Configuration;
using TraWell.Models;

namespace TraWell.Services;

public class PlaceService
{
    private static readonly List<Place> _places = new()
    {
        new Place
        {
            Id = 1,
            Name = "Ladakh",
            Description = "Known for its stunning landscapes, Buddhist monasteries, and high-altitude desert.",
            Latitude = 34.2268,
            Longitude = 77.5619,
            Categories = new List<string> { "hiking", "culture", "wildlife" },
            BestSeasons = new List<string> { "summer" },
            RecommendedDuration = 7,
            Rating = 4.8
        },
        new Place
        {
            Id = 2,
            Name = "Rishikesh",
            Description = "The yoga capital of the world, known for adventure sports and spiritual activities.",
            Latitude = 30.0869,
            Longitude = 78.2676,
            Categories = new List<string> { "water-sports", "culture" },
            BestSeasons = new List<string> { "winter", "summer" },
            RecommendedDuration = 3,
            Rating = 4.6
        },
        new Place
        {
            Id = 3,
            Name = "Kerala Backwaters",
            Description = "A network of lagoons, lakes, and canals parallel to the Arabian Sea coast.",
            Latitude = 9.4981,
            Longitude = 76.3388,
            Categories = new List<string> { "water-sports", "culture" },
            BestSeasons = new List<string> { "winter" },
            RecommendedDuration = 2,
            Rating = 4.7
        }
    };

    public PlaceService(IConfiguration configuration)
    {
    }

    public Task<List<Place>> GetAllPlaces()
    {
        return Task.FromResult(_places.ToList());
    }

    public Task<Place> GetPlaceById(int id)
    {
        var place = _places.FirstOrDefault(p => p.Id == id) ?? 
            throw new KeyNotFoundException($"Place with ID {id} not found");
        return Task.FromResult(place);
    }
}

using System.ComponentModel.DataAnnotations;

namespace TraWell.Models.DTOs
{
    public class CreatePackageRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 365)]
        public int Duration { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Difficulty { get; set; } = "Easy";

        [Range(1, 100)]
        public int MaxGroupSize { get; set; } = 20;

        public List<string> Inclusions { get; set; } = new();

        public List<string> Exclusions { get; set; } = new();

        public List<string> ImageUrls { get; set; } = new();

        public string Itinerary { get; set; } = string.Empty;

        [Required]
        public int PlaceId { get; set; }

        [Required]
        public int ProviderId { get; set; }
    }

    public class UpdatePackageRequest : CreatePackageRequest
    {
        // Inherits all properties from CreatePackageRequest
    }

    public class CreateHotelRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Range(1, 5)]
        public int StarRating { get; set; } = 1;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PricePerNight { get; set; }

        public List<string> Amenities { get; set; } = new();

        public List<string> ImageUrls { get; set; } = new();

        [Required]
        [MaxLength(50)]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public bool HasWifi { get; set; } = false;
        public bool HasParking { get; set; } = false;
        public bool HasSwimmingPool { get; set; } = false;
        public bool HasGym { get; set; } = false;
        public bool HasSpa { get; set; } = false;
        public bool PetFriendly { get; set; } = false;

        [Range(1, 1000)]
        public int TotalRooms { get; set; } = 10;

        [Required]
        public int PlaceId { get; set; }
    }

    public class UpdateHotelRequest : CreateHotelRequest
    {
        // Inherits all properties from CreateHotelRequest
    }

    public class CreatePlaceRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }

        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();

        public List<string> ImageUrls { get; set; } = new();

        public List<string> Categories { get; set; } = new();
        public List<string> BestSeasons { get; set; } = new();

        [Range(1, 30)]
        public int RecommendedDuration { get; set; } = 3;
    }

    public class UpdatePlaceRequest : CreatePlaceRequest
    {
        // Inherits all properties from CreatePlaceRequest
    }
}

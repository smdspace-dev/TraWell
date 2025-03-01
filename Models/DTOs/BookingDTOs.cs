using System.ComponentModel.DataAnnotations;

namespace TraWell.Models.DTOs
{
    public class CreatePackageBookingRequest
    {
        [Required]
        public int TourPackageId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [Range(1, 50)]
        public int NumberOfPeople { get; set; }

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }
    }

    public class CreateHotelBookingRequest
    {
        [Required]
        public int HotelId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, 50)]
        public int NumberOfPeople { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfRooms { get; set; }

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }
    }

    public class CreateReviewRequest
    {
        public int? TourPackageId { get; set; }
        public int? HotelId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new();
    }

    public class SearchRequest
    {
        public string? Destination { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Guests { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRating { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class PackageSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public PlaceSearchResult Place { get; set; } = new();
        public ProviderSearchResult Provider { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class HotelSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int StarRating { get; set; }
        public List<string> Amenities { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public PlaceSearchResult Place { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class PlaceSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public double Rating { get; set; }
    }

    public class ProviderSearchResult
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}

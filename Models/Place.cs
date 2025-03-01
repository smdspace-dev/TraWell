using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class Place
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Place name is required")]
        [MaxLength(100, ErrorMessage = "Place name cannot exceed 100 characters")]
        [MinLength(2, ErrorMessage = "Place name must be at least 2 characters")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Country is required")]
        [MaxLength(50, ErrorMessage = "Country name cannot exceed 50 characters")]
        public string Country { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "State name cannot exceed 50 characters")]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;
        
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }
        
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }
        
        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty; // Beach, Mountain, Cultural, etc.
        
        public List<string> Tags { get; set; } = new List<string>(); // hiking, surfing, etc.
        
        public List<string> ImageUrls { get; set; } = new List<string>();
        
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0 and 5")]
        public double Rating { get; set; } = 0.0;
        
        [Range(0, int.MaxValue, ErrorMessage = "Review count must be non-negative")]
        public int ReviewCount { get; set; } = 0;

        public List<string> Categories { get; set; } = new List<string>(); // For backward compatibility
        public List<string> BestSeasons { get; set; } = new List<string>();
        
        [Range(1, 30, ErrorMessage = "Recommended duration must be between 1 and 30 days")]
        public int RecommendedDuration { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TourPackage> TourPackages { get; set; } = new List<TourPackage>();
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class TourPackage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        [MaxLength(200, ErrorMessage = "Package name cannot exceed 200 characters")]
        [MinLength(5, ErrorMessage = "Package name must be at least 5 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int Duration { get; set; } // Duration in days

        [Required(ErrorMessage = "Category is required")]
        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty; // Adventure, Cultural, Beach, etc.

        [MaxLength(100, ErrorMessage = "Difficulty cannot exceed 100 characters")]
        public string Difficulty { get; set; } = "Easy"; // Easy, Moderate, Difficult

        [Range(1, 100, ErrorMessage = "Max group size must be between 1 and 100")]
        public int MaxGroupSize { get; set; } = 20;

        public List<string> Inclusions { get; set; } = new List<string>();

        public List<string> Exclusions { get; set; } = new List<string>();

        public List<string> ImageUrls { get; set; } = new List<string>();

        public string Itinerary { get; set; } = string.Empty; // JSON string or detailed text

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int PlaceId { get; set; }
        public int ProviderId { get; set; }

        // Navigation properties
        public Place Place { get; set; } = null!;
        public PackageProvider Provider { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

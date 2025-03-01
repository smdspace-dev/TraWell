using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

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

        public List<string> ImageUrls { get; set; } = new List<string>();

        public bool IsVerified { get; set; } = false;

        public bool IsVisible { get; set; } = true;

        public int HelpfulCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public TourPackage? TourPackage { get; set; }
        public Hotel? Hotel { get; set; }
    }
}

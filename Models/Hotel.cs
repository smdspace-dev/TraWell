using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hotel name is required")]
        [MaxLength(200, ErrorMessage = "Hotel name cannot exceed 200 characters")]
        [MinLength(2, ErrorMessage = "Hotel name must be at least 2 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
        public string Address { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5")]
        public int StarRating { get; set; } = 1;

        [Required(ErrorMessage = "Price per night is required")]
        [Range(0.01, 99999.99, ErrorMessage = "Price per night must be between $0.01 and $99,999.99")]
        public decimal PricePerNight { get; set; }

        public List<string> Amenities { get; set; } = new List<string>();

        public List<string> ImageUrls { get; set; } = new List<string>();

        [Required(ErrorMessage = "Contact phone is required")]
        [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ContactEmail { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid website URL")]
        public string Website { get; set; } = string.Empty;

        public bool HasWifi { get; set; } = false;
        public bool HasParking { get; set; } = false;
        public bool HasSwimmingPool { get; set; } = false;
        public bool HasGym { get; set; } = false;
        public bool HasSpa { get; set; } = false;
        public bool PetFriendly { get; set; } = false;

        [Range(1, 5000, ErrorMessage = "Total rooms must be between 1 and 5000")]
        public int TotalRooms { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int PlaceId { get; set; }

        // Navigation properties
        public Place Place { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

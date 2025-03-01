using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? TourPackageId { get; set; }
        public int? HotelId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public int NumberOfPeople { get; set; } = 1;

        public int NumberOfRooms { get; set; } = 0; // For hotel bookings

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded

        [MaxLength(100)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string SpecialRequests { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public TourPackage? TourPackage { get; set; }
        public Hotel? Hotel { get; set; }
    }
}

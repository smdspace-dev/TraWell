using System.ComponentModel.DataAnnotations;

namespace TraWell.Models.DTOs
{
    public class CreatePaymentOrderRequest
    {
        public int? PackageId { get; set; }
        public int? HotelId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; } = 1;

        [Required]
        [Range(1, 10)]
        public int NumberOfRooms { get; set; } = 1;

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = "";

        [Required]
        public string CustomerPhone { get; set; } = "";

        public string? SpecialRequests { get; set; }
    }

    public class VerifyPaymentRequest
    {
        [Required]
        public string PaymentId { get; set; } = "";

        [Required]
        public string OrderId { get; set; } = "";

        [Required]
        public string Signature { get; set; } = "";
    }

    public class PaymentOrderResponse
    {
        public string Id { get; set; } = "";
        public string Entity { get; set; } = "";
        public int Amount { get; set; }
        public int AmountPaid { get; set; }
        public int AmountDue { get; set; }
        public string Currency { get; set; } = "";
        public string Receipt { get; set; } = "";
        public string Status { get; set; } = "";
        public long CreatedAt { get; set; }
        public Dictionary<string, object>? Notes { get; set; }
    }

    public class PaymentDetailsResponse
    {
        public string Id { get; set; } = "";
        public string Entity { get; set; } = "";
        public int Amount { get; set; }
        public string Currency { get; set; } = "";
        public string Status { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string Method { get; set; } = "";
        public long CreatedAt { get; set; }
        public Dictionary<string, object>? Notes { get; set; }
    }
}

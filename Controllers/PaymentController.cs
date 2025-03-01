using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraWell.Services;
using TraWell.Models.DTOs;
using TraWell.Data;
using TraWell.Models;
using Microsoft.EntityFrameworkCore;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly TraWellDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, TraWellDbContext context, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("create-order")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentOrder([FromBody] CreatePaymentOrderRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Verify the package/hotel exists and get details
                decimal amount = 0;
                string itemName = "";
                string itemType = "";

                if (request.PackageId.HasValue)
                {
                    var package = await _context.TourPackages
                        .Include(p => p.Place)
                        .FirstOrDefaultAsync(p => p.Id == request.PackageId.Value && p.IsActive);
                    
                    if (package == null)
                        return NotFound(new { message = "Package not found" });

                    amount = package.Price;
                    itemName = package.Name;
                    itemType = "package";
                }
                else if (request.HotelId.HasValue)
                {
                    var hotel = await _context.Hotels
                        .Include(h => h.Place)
                        .FirstOrDefaultAsync(h => h.Id == request.HotelId.Value && h.IsActive);
                    
                    if (hotel == null)
                        return NotFound(new { message = "Hotel not found" });

                    // Calculate total amount based on nights and rooms
                    var nights = (request.CheckOutDate - request.CheckInDate).Days;
                    amount = hotel.PricePerNight * nights * request.NumberOfRooms;
                    itemName = hotel.Name;
                    itemType = "hotel";
                }
                else
                {
                    return BadRequest(new { message = "Either PackageId or HotelId is required" });
                }

                // Create payment order
                var receipt = $"rcpt_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}";
                var metadata = new Dictionary<string, object>
                {
                    ["user_id"] = userId,
                    ["item_type"] = itemType,
                    ["item_name"] = itemName,
                    ["package_id"] = request.PackageId?.ToString() ?? "",
                    ["hotel_id"] = request.HotelId?.ToString() ?? "",
                    ["guests"] = request.NumberOfGuests.ToString(),
                    ["rooms"] = request.NumberOfRooms.ToString()
                };

                var order = await _paymentService.CreateOrderAsync(amount, "INR", receipt, metadata);

                // Store booking as pending payment
                var booking = new Booking
                {
                    UserId = userId,
                    TourPackageId = request.PackageId,
                    HotelId = request.HotelId,
                    BookingDate = DateTime.UtcNow,
                    StartDate = request.CheckInDate,
                    EndDate = request.CheckOutDate,
                    NumberOfPeople = request.NumberOfGuests,
                    NumberOfRooms = request.NumberOfRooms,
                    TotalAmount = amount,
                    Status = "PendingPayment",
                    PaymentStatus = "Pending",
                    PaymentMethod = "Razorpay",
                    TransactionId = order.Id,
                    SpecialRequests = request.SpecialRequests ?? "",
                    Notes = $"Razorpay Order ID: {order.Id}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    orderId = order.Id,
                    amount = order.Amount,
                    currency = order.Currency,
                    keyId = "rzp_test_demo_key_123456", // From configuration
                    bookingId = booking.Id,
                    itemName = itemName,
                    customerName = request.CustomerName,
                    customerEmail = request.CustomerEmail,
                    customerPhone = request.CustomerPhone
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("verify-payment")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Verify payment signature
                var isValid = await _paymentService.VerifyPaymentAsync(
                    request.PaymentId, 
                    request.OrderId, 
                    request.Signature
                );

                if (!isValid)
                {
                    return BadRequest(new { message = "Invalid payment signature" });
                }

                // Update booking status
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.TransactionId == request.OrderId && b.UserId == userId);

                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                booking.Status = "Confirmed";
                booking.PaymentStatus = "Completed";
                booking.TransactionId = request.PaymentId;
                booking.Notes += $" | Payment ID: {request.PaymentId}";
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Payment verified successfully",
                    bookingId = booking.Id,
                    status = "Confirmed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentDetails(string orderId)
        {
            try
            {
                var paymentDetails = await _paymentService.GetPaymentDetailsAsync(orderId);
                return Ok(paymentDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment details for order {OrderId}", orderId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

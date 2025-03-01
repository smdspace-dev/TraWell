using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TraWell.Data;
using TraWell.Models;
using TraWell.Models.DTOs;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(TraWellDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("package")]
        [Authorize]
        public async Task<ActionResult<Booking>> BookPackage([FromBody] CreatePackageBookingRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var package = await _context.TourPackages.FindAsync(request.TourPackageId);
                if (package == null || !package.IsActive)
                {
                    return NotFound(new { message = "Tour package not found or not available" });
                }

                var booking = new Booking
                {
                    UserId = userId,
                    TourPackageId = request.TourPackageId,
                    BookingDate = DateTime.UtcNow,
                    StartDate = request.StartDate,
                    EndDate = request.StartDate.AddDays(package.Duration),
                    NumberOfPeople = request.NumberOfPeople,
                    TotalAmount = package.Price * request.NumberOfPeople,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    SpecialRequests = request.SpecialRequests ?? string.Empty
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(booking)
                    .Reference(b => b.TourPackage)
                    .LoadAsync();

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking package");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("hotel")]
        [Authorize]
        public async Task<ActionResult<Booking>> BookHotel([FromBody] CreateHotelBookingRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var hotel = await _context.Hotels.FindAsync(request.HotelId);
                if (hotel == null || !hotel.IsActive)
                {
                    return NotFound(new { message = "Hotel not found or not available" });
                }

                var nights = (request.EndDate - request.StartDate).Days;
                if (nights <= 0)
                {
                    return BadRequest(new { message = "Invalid date range" });
                }

                var booking = new Booking
                {
                    UserId = userId,
                    HotelId = request.HotelId,
                    BookingDate = DateTime.UtcNow,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    NumberOfPeople = request.NumberOfPeople,
                    NumberOfRooms = request.NumberOfRooms,
                    TotalAmount = hotel.PricePerNight * nights * request.NumberOfRooms,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    SpecialRequests = request.SpecialRequests ?? string.Empty
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(booking)
                    .Reference(b => b.Hotel)
                    .LoadAsync();

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking hotel");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var bookings = await _context.Bookings
                    .Where(b => b.UserId == userId)
                    .Include(b => b.TourPackage)
                    .Include(b => b.Hotel)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var booking = await _context.Bookings
                    .Include(b => b.TourPackage)
                        .ThenInclude(p => p!.Place)
                    .Include(b => b.Hotel)
                        .ThenInclude(h => h!.Place)
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<ActionResult> CancelBooking(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                if (booking.Status == "Cancelled")
                {
                    return BadRequest(new { message = "Booking is already cancelled" });
                }

                if (booking.Status == "Completed")
                {
                    return BadRequest(new { message = "Cannot cancel completed booking" });
                }

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

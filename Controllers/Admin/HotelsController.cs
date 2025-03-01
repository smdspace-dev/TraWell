using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraWell.Data;
using TraWell.Models;
using TraWell.Models.DTOs;

namespace TraWell.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class HotelsController : ControllerBase
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(TraWellDbContext context, ILogger<HotelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? city = null, [FromQuery] int? starRating = null)
        {
            try
            {
                var query = _context.Hotels.Include(h => h.Place).AsQueryable();

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(h => h.Place.City.ToLower().Contains(city.ToLower()));
                }

                if (starRating.HasValue)
                {
                    query = query.Where(h => h.StarRating == starRating.Value);
                }

                var hotels = await query
                    .OrderByDescending(h => h.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var total = await query.CountAsync();

                return Ok(new
                {
                    data = hotels,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotels");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Hotel>> GetHotel(int id)
        {
            try
            {
                var hotel = await _context.Hotels
                    .Include(h => h.Place)
                    .Include(h => h.Reviews)
                    .Include(h => h.Bookings)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hotel == null)
                {
                    return NotFound(new { message = "Hotel not found" });
                }

                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel {HotelId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Hotel>> CreateHotel([FromBody] CreateHotelRequest request)
        {
            try
            {
                // Verify place exists
                var place = await _context.Places.FindAsync(request.PlaceId);
                if (place == null)
                {
                    return BadRequest(new { message = "Place not found" });
                }

                var hotel = new Hotel
                {
                    Name = request.Name,
                    Description = request.Description,
                    Address = request.Address,
                    StarRating = request.StarRating,
                    PricePerNight = request.PricePerNight,
                    Amenities = request.Amenities,
                    ImageUrls = request.ImageUrls,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    Website = request.Website,
                    HasWifi = request.HasWifi,
                    HasParking = request.HasParking,
                    HasSwimmingPool = request.HasSwimmingPool,
                    HasGym = request.HasGym,
                    HasSpa = request.HasSpa,
                    PetFriendly = request.PetFriendly,
                    TotalRooms = request.TotalRooms,
                    PlaceId = request.PlaceId,
                    IsActive = true
                };

                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();

                // Load the related data
                await _context.Entry(hotel).Reference(h => h.Place).LoadAsync();

                return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelRequest request)
        {
            try
            {
                var hotel = await _context.Hotels.FindAsync(id);
                if (hotel == null)
                {
                    return NotFound(new { message = "Hotel not found" });
                }

                // Verify place exists if it's being changed
                if (hotel.PlaceId != request.PlaceId)
                {
                    var place = await _context.Places.FindAsync(request.PlaceId);
                    if (place == null)
                    {
                        return BadRequest(new { message = "Place not found" });
                    }
                }

                hotel.Name = request.Name;
                hotel.Description = request.Description;
                hotel.Address = request.Address;
                hotel.StarRating = request.StarRating;
                hotel.PricePerNight = request.PricePerNight;
                hotel.Amenities = request.Amenities;
                hotel.ImageUrls = request.ImageUrls;
                hotel.ContactPhone = request.ContactPhone;
                hotel.ContactEmail = request.ContactEmail;
                hotel.Website = request.Website;
                hotel.HasWifi = request.HasWifi;
                hotel.HasParking = request.HasParking;
                hotel.HasSwimmingPool = request.HasSwimmingPool;
                hotel.HasGym = request.HasGym;
                hotel.HasSpa = request.HasSpa;
                hotel.PetFriendly = request.PetFriendly;
                hotel.TotalRooms = request.TotalRooms;
                hotel.PlaceId = request.PlaceId;
                hotel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Hotel updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel {HotelId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            try
            {
                var hotel = await _context.Hotels.FindAsync(id);
                if (hotel == null)
                {
                    return NotFound(new { message = "Hotel not found" });
                }

                hotel.IsActive = false; // Soft delete
                hotel.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Hotel deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel {HotelId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleHotelStatus(int id)
        {
            try
            {
                var hotel = await _context.Hotels.FindAsync(id);
                if (hotel == null)
                {
                    return NotFound(new { message = "Hotel not found" });
                }

                hotel.IsActive = !hotel.IsActive;
                hotel.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"Hotel {(hotel.IsActive ? "activated" : "deactivated")} successfully",
                    isActive = hotel.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling hotel status {HotelId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetHotelStatistics()
        {
            try
            {
                var totalHotels = await _context.Hotels.CountAsync();
                var activeHotels = await _context.Hotels.CountAsync(h => h.IsActive);
                var avgRating = await _context.Reviews
                    .Where(r => r.HotelId != null)
                    .AverageAsync(r => (double?)r.Rating) ?? 0;
                var totalBookings = await _context.Bookings
                    .Where(b => b.HotelId != null)
                    .CountAsync();

                return Ok(new
                {
                    totalHotels = totalHotels,
                    activeHotels = activeHotels,
                    inactiveHotels = totalHotels - activeHotels,
                    averageRating = Math.Round(avgRating, 2),
                    totalBookings = totalBookings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

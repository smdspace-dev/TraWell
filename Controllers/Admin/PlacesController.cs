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
    public class PlacesController : ControllerBase
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<PlacesController> _logger;

        public PlacesController(TraWellDbContext context, ILogger<PlacesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Place>>> GetPlaces([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? country = null, [FromQuery] string? category = null)
        {
            try
            {
                var query = _context.Places.AsQueryable();

                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(p => p.Country.ToLower().Contains(country.ToLower()));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
                }

                var places = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var total = await query.CountAsync();

                return Ok(new
                {
                    data = places,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting places");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Place>> GetPlace(int id)
        {
            try
            {
                var place = await _context.Places
                    .Include(p => p.TourPackages)
                    .Include(p => p.Hotels)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                {
                    return NotFound(new { message = "Place not found" });
                }

                return Ok(place);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting place {PlaceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Place>> CreatePlace([FromBody] CreatePlaceRequest request)
        {
            try
            {
                var place = new Place
                {
                    Name = request.Name,
                    Description = request.Description,
                    Country = request.Country,
                    State = request.State,
                    City = request.City,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Category = request.Category,
                    Tags = request.Tags,
                    ImageUrls = request.ImageUrls,
                    Categories = request.Categories,
                    BestSeasons = request.BestSeasons,
                    RecommendedDuration = request.RecommendedDuration,
                    Rating = 0.0,
                    ReviewCount = 0,
                    IsActive = true
                };

                _context.Places.Add(place);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlace), new { id = place.Id }, place);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating place");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] UpdatePlaceRequest request)
        {
            try
            {
                var place = await _context.Places.FindAsync(id);
                if (place == null)
                {
                    return NotFound(new { message = "Place not found" });
                }

                place.Name = request.Name;
                place.Description = request.Description;
                place.Country = request.Country;
                place.State = request.State;
                place.City = request.City;
                place.Latitude = request.Latitude;
                place.Longitude = request.Longitude;
                place.Category = request.Category;
                place.Tags = request.Tags;
                place.ImageUrls = request.ImageUrls;
                place.Categories = request.Categories;
                place.BestSeasons = request.BestSeasons;
                place.RecommendedDuration = request.RecommendedDuration;
                place.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Place updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating place {PlaceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            try
            {
                var place = await _context.Places
                    .Include(p => p.TourPackages)
                    .Include(p => p.Hotels)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                {
                    return NotFound(new { message = "Place not found" });
                }

                if (place.TourPackages.Any() || place.Hotels.Any())
                {
                    return BadRequest(new { message = "Cannot delete place with associated packages or hotels" });
                }

                place.IsActive = false; // Soft delete
                place.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Place deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting place {PlaceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetPlaceStatistics()
        {
            try
            {
                var totalPlaces = await _context.Places.CountAsync();
                var activePlaces = await _context.Places.CountAsync(p => p.IsActive);
                var totalPackages = await _context.TourPackages.CountAsync();
                var totalHotels = await _context.Hotels.CountAsync();
                
                var countryCounts = await _context.Places
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Country)
                    .Select(g => new { Country = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                return Ok(new
                {
                    totalPlaces = totalPlaces,
                    activePlaces = activePlaces,
                    inactivePlaces = totalPlaces - activePlaces,
                    totalPackages = totalPackages,
                    totalHotels = totalHotels,
                    topCountries = countryCounts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting place statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("countries")]
        public async Task<ActionResult> GetCountries()
        {
            try
            {
                var countries = await _context.Places
                    .Where(p => p.IsActive)
                    .Select(p => p.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Places
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

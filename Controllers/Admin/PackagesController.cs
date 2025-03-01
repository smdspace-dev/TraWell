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
    public class PackagesController : ControllerBase
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<PackagesController> _logger;

        public PackagesController(TraWellDbContext context, ILogger<PackagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourPackage>>> GetPackages([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var packages = await _context.TourPackages
                    .Include(p => p.Place)
                    .Include(p => p.Provider)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var total = await _context.TourPackages.CountAsync();

                return Ok(new
                {
                    data = packages,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting packages");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TourPackage>> GetPackage(int id)
        {
            try
            {
                var package = await _context.TourPackages
                    .Include(p => p.Place)
                    .Include(p => p.Provider)
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TourPackage>> CreatePackage([FromBody] CreatePackageRequest request)
        {
            try
            {
                var package = new TourPackage
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Duration = request.Duration,
                    Category = request.Category,
                    Difficulty = request.Difficulty,
                    MaxGroupSize = request.MaxGroupSize,
                    Inclusions = request.Inclusions,
                    Exclusions = request.Exclusions,
                    ImageUrls = request.ImageUrls,
                    Itinerary = request.Itinerary,
                    PlaceId = request.PlaceId,
                    ProviderId = request.ProviderId,
                    IsActive = true
                };

                _context.TourPackages.Add(package);
                await _context.SaveChangesAsync();

                // Load the related data
                await _context.Entry(package)
                    .Reference(p => p.Place)
                    .LoadAsync();
                await _context.Entry(package)
                    .Reference(p => p.Provider)
                    .LoadAsync();

                return CreatedAtAction(nameof(GetPackage), new { id = package.Id }, package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating package");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] UpdatePackageRequest request)
        {
            try
            {
                var package = await _context.TourPackages.FindAsync(id);
                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                package.Name = request.Name;
                package.Description = request.Description;
                package.Price = request.Price;
                package.Duration = request.Duration;
                package.Category = request.Category;
                package.Difficulty = request.Difficulty;
                package.MaxGroupSize = request.MaxGroupSize;
                package.Inclusions = request.Inclusions;
                package.Exclusions = request.Exclusions;
                package.ImageUrls = request.ImageUrls;
                package.Itinerary = request.Itinerary;
                package.PlaceId = request.PlaceId;
                package.ProviderId = request.ProviderId;
                package.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Package updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                var package = await _context.TourPackages.FindAsync(id);
                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                package.IsActive = false; // Soft delete
                package.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Package deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> TogglePackageStatus(int id)
        {
            try
            {
                var package = await _context.TourPackages.FindAsync(id);
                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                package.IsActive = !package.IsActive;
                package.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"Package {(package.IsActive ? "activated" : "deactivated")} successfully",
                    isActive = package.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling package status {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

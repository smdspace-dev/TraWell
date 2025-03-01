using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraWell.Data;
using TraWell.Models.DTOs;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<PackageSearchResult>>> GetPackages([FromQuery] SearchRequest? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            try
            {
                var query = _context.TourPackages
                    .Where(p => p.IsActive)
                    .Include(p => p.Place)
                    .Include(p => p.Provider)
                    .Include(p => p.Reviews)
                    .AsQueryable();

                // Apply search filters
                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.Destination))
                    {
                        query = query.Where(p => p.Place.Name.Contains(search.Destination) ||
                                                p.Place.City.Contains(search.Destination) ||
                                                p.Place.Country.Contains(search.Destination));
                    }

                    if (!string.IsNullOrEmpty(search.Category))
                    {
                        query = query.Where(p => p.Category.ToLower().Contains(search.Category.ToLower()));
                    }

                    if (search.MinPrice.HasValue)
                    {
                        query = query.Where(p => p.Price >= search.MinPrice.Value);
                    }

                    if (search.MaxPrice.HasValue)
                    {
                        query = query.Where(p => p.Price <= search.MaxPrice.Value);
                    }

                    if (search.Tags.Any())
                    {
                        // This is a simplified tag search - in production you'd want more sophisticated matching
                        foreach (var tag in search.Tags)
                        {
                            query = query.Where(p => p.Place.Tags.Any(t => t.ToLower().Contains(tag.ToLower())));
                        }
                    }
                }

                var packages = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PackageSearchResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Duration = p.Duration,
                        Category = p.Category,
                        Difficulty = p.Difficulty,
                        ImageUrls = p.ImageUrls,
                        Place = new PlaceSearchResult
                        {
                            Id = p.Place.Id,
                            Name = p.Place.Name,
                            Country = p.Place.Country,
                            State = p.Place.State,
                            City = p.Place.City,
                            Category = p.Place.Category,
                            Tags = p.Place.Tags,
                            Rating = p.Place.Rating
                        },
                        Provider = new ProviderSearchResult
                        {
                            Id = p.Provider.Id,
                            CompanyName = p.Provider.CompanyName,
                            IsVerified = p.Provider.IsVerified
                        },
                        AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = p.Reviews.Count
                    })
                    .ToListAsync();

                var total = await query.CountAsync();

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
        public async Task<ActionResult<PackageSearchResult>> GetPackage(int id)
        {
            try
            {
                var package = await _context.TourPackages
                    .Where(p => p.Id == id && p.IsActive)
                    .Include(p => p.Place)
                    .Include(p => p.Provider)
                    .Include(p => p.Reviews)
                        .ThenInclude(r => r.User)
                    .Select(p => new
                    {
                        Package = p,
                        Reviews = p.Reviews.Where(r => r.IsVisible).Select(r => new
                        {
                            r.Id,
                            r.Rating,
                            r.Title,
                            r.Comment,
                            r.CreatedAt,
                            UserName = r.User.FirstName + " " + r.User.LastName,
                            r.ImageUrls,
                            r.IsVerified
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (package == null)
                {
                    return NotFound(new { message = "Package not found" });
                }

                var result = new PackageSearchResult
                {
                    Id = package.Package.Id,
                    Name = package.Package.Name,
                    Description = package.Package.Description,
                    Price = package.Package.Price,
                    Duration = package.Package.Duration,
                    Category = package.Package.Category,
                    Difficulty = package.Package.Difficulty,
                    ImageUrls = package.Package.ImageUrls,
                    Place = new PlaceSearchResult
                    {
                        Id = package.Package.Place.Id,
                        Name = package.Package.Place.Name,
                        Country = package.Package.Place.Country,
                        State = package.Package.Place.State,
                        City = package.Package.Place.City,
                        Category = package.Package.Place.Category,
                        Tags = package.Package.Place.Tags,
                        Rating = package.Package.Place.Rating
                    },
                    Provider = new ProviderSearchResult
                    {
                        Id = package.Package.Provider.Id,
                        CompanyName = package.Package.Provider.CompanyName,
                        IsVerified = package.Package.Provider.IsVerified
                    },
                    AverageRating = package.Reviews.Any() ? package.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = package.Reviews.Count
                };

                return Ok(new
                {
                    package = result,
                    reviews = package.Reviews,
                    inclusions = package.Package.Inclusions,
                    exclusions = package.Package.Exclusions,
                    itinerary = package.Package.Itinerary,
                    maxGroupSize = package.Package.MaxGroupSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package details");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<PackageSearchResult>>> GetFeaturedPackages([FromQuery] int count = 6)
        {
            try
            {
                var packages = await _context.TourPackages
                    .Where(p => p.IsActive)
                    .Include(p => p.Place)
                    .Include(p => p.Provider)
                    .Include(p => p.Reviews)
                    .OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0)
                    .Take(count)
                    .Select(p => new PackageSearchResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Duration = p.Duration,
                        Category = p.Category,
                        Difficulty = p.Difficulty,
                        ImageUrls = p.ImageUrls,
                        Place = new PlaceSearchResult
                        {
                            Id = p.Place.Id,
                            Name = p.Place.Name,
                            Country = p.Place.Country,
                            State = p.Place.State,
                            City = p.Place.City,
                            Category = p.Place.Category,
                            Tags = p.Place.Tags,
                            Rating = p.Place.Rating
                        },
                        Provider = new ProviderSearchResult
                        {
                            Id = p.Provider.Id,
                            CompanyName = p.Provider.CompanyName,
                            IsVerified = p.Provider.IsVerified
                        },
                        AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = p.Reviews.Count
                    })
                    .ToListAsync();

                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured packages");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _context.TourPackages
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package categories");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

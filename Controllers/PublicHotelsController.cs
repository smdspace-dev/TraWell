using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraWell.Data;
using TraWell.Models.DTOs;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<HotelSearchResult>>> GetHotels([FromQuery] SearchRequest? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            try
            {
                var query = _context.Hotels
                    .Where(h => h.IsActive)
                    .Include(h => h.Place)
                    .Include(h => h.Reviews)
                    .AsQueryable();

                // Apply search filters
                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.Destination))
                    {
                        query = query.Where(h => h.Place.Name.Contains(search.Destination) ||
                                                h.Place.City.Contains(search.Destination) ||
                                                h.Place.Country.Contains(search.Destination) ||
                                                h.Name.Contains(search.Destination));
                    }

                    if (search.MinPrice.HasValue)
                    {
                        query = query.Where(h => h.PricePerNight >= search.MinPrice.Value);
                    }

                    if (search.MaxPrice.HasValue)
                    {
                        query = query.Where(h => h.PricePerNight <= search.MaxPrice.Value);
                    }

                    if (search.MinRating.HasValue)
                    {
                        query = query.Where(h => h.StarRating >= search.MinRating.Value);
                    }
                }

                var hotels = await query
                    .OrderByDescending(h => h.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new HotelSearchResult
                    {
                        Id = h.Id,
                        Name = h.Name,
                        Description = h.Description,
                        PricePerNight = h.PricePerNight,
                        StarRating = h.StarRating,
                        Amenities = h.Amenities,
                        ImageUrls = h.ImageUrls,
                        Place = new PlaceSearchResult
                        {
                            Id = h.Place.Id,
                            Name = h.Place.Name,
                            Country = h.Place.Country,
                            State = h.Place.State,
                            City = h.Place.City,
                            Category = h.Place.Category,
                            Tags = h.Place.Tags,
                            Rating = h.Place.Rating
                        },
                        AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = h.Reviews.Count
                    })
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
        public async Task<ActionResult<HotelSearchResult>> GetHotel(int id)
        {
            try
            {
                var hotel = await _context.Hotels
                    .Where(h => h.Id == id && h.IsActive)
                    .Include(h => h.Place)
                    .Include(h => h.Reviews)
                        .ThenInclude(r => r.User)
                    .Select(h => new
                    {
                        Hotel = h,
                        Reviews = h.Reviews.Where(r => r.IsVisible).Select(r => new
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

                if (hotel == null)
                {
                    return NotFound(new { message = "Hotel not found" });
                }

                var result = new HotelSearchResult
                {
                    Id = hotel.Hotel.Id,
                    Name = hotel.Hotel.Name,
                    Description = hotel.Hotel.Description,
                    PricePerNight = hotel.Hotel.PricePerNight,
                    StarRating = hotel.Hotel.StarRating,
                    Amenities = hotel.Hotel.Amenities,
                    ImageUrls = hotel.Hotel.ImageUrls,
                    Place = new PlaceSearchResult
                    {
                        Id = hotel.Hotel.Place.Id,
                        Name = hotel.Hotel.Place.Name,
                        Country = hotel.Hotel.Place.Country,
                        State = hotel.Hotel.Place.State,
                        City = hotel.Hotel.Place.City,
                        Category = hotel.Hotel.Place.Category,
                        Tags = hotel.Hotel.Place.Tags,
                        Rating = hotel.Hotel.Place.Rating
                    },
                    AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = hotel.Reviews.Count
                };

                return Ok(new
                {
                    hotel = result,
                    reviews = hotel.Reviews,
                    address = hotel.Hotel.Address,
                    contactPhone = hotel.Hotel.ContactPhone,
                    contactEmail = hotel.Hotel.ContactEmail,
                    website = hotel.Hotel.Website,
                    features = new
                    {
                        hasWifi = hotel.Hotel.HasWifi,
                        hasParking = hotel.Hotel.HasParking,
                        hasSwimmingPool = hotel.Hotel.HasSwimmingPool,
                        hasGym = hotel.Hotel.HasGym,
                        hasSpa = hotel.Hotel.HasSpa,
                        petFriendly = hotel.Hotel.PetFriendly
                    },
                    totalRooms = hotel.Hotel.TotalRooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel details");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<HotelSearchResult>>> GetFeaturedHotels([FromQuery] int count = 6)
        {
            try
            {
                var hotels = await _context.Hotels
                    .Where(h => h.IsActive)
                    .Include(h => h.Place)
                    .Include(h => h.Reviews)
                    .OrderByDescending(h => h.StarRating)
                    .ThenByDescending(h => h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0)
                    .Take(count)
                    .Select(h => new HotelSearchResult
                    {
                        Id = h.Id,
                        Name = h.Name,
                        Description = h.Description,
                        PricePerNight = h.PricePerNight,
                        StarRating = h.StarRating,
                        Amenities = h.Amenities,
                        ImageUrls = h.ImageUrls,
                        Place = new PlaceSearchResult
                        {
                            Id = h.Place.Id,
                            Name = h.Place.Name,
                            Country = h.Place.Country,
                            State = h.Place.State,
                            City = h.Place.City,
                            Category = h.Place.Category,
                            Tags = h.Place.Tags,
                            Rating = h.Place.Rating
                        },
                        AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = h.Reviews.Count
                    })
                    .ToListAsync();

                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured hotels");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("destinations")]
        public async Task<ActionResult<IEnumerable<object>>> GetTopDestinations([FromQuery] int count = 10)
        {
            try
            {
                var destinations = await _context.Hotels
                    .Where(h => h.IsActive)
                    .Include(h => h.Place)
                    .GroupBy(h => new { h.Place.City, h.Place.Country })
                    .Select(g => new
                    {
                        city = g.Key.City,
                        country = g.Key.Country,
                        hotelCount = g.Count(),
                        averagePrice = g.Average(h => h.PricePerNight),
                        place = g.First().Place
                    })
                    .OrderByDescending(x => x.hotelCount)
                    .Take(count)
                    .ToListAsync();

                return Ok(destinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top destinations");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

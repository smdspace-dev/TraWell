using TraWell.Data;
using TraWell.Models;
using Microsoft.EntityFrameworkCore;

namespace TraWell.Services
{
    public class DataSeeder
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(TraWellDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedSampleDataAsync()
        {
            try
            {
                // Seed Places
                if (!await _context.Places.AnyAsync())
                {
                    await SeedPlacesAsync();
                }

                // Seed Package Providers
                if (!await _context.PackageProviders.AnyAsync())
                {
                    await SeedPackageProvidersAsync();
                }

                // Seed Hotels
                if (!await _context.Hotels.AnyAsync())
                {
                    await SeedHotelsAsync();
                }

                // Seed Tour Packages
                if (!await _context.TourPackages.AnyAsync())
                {
                    await SeedTourPackagesAsync();
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Sample data seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding sample data");
            }
        }

        private async Task SeedPlacesAsync()
        {
            var places = new List<Place>
            {
                new Place
                {
                    Name = "Bali Paradise",
                    Description = "Tropical paradise with stunning beaches, ancient temples, and lush rice terraces. Experience the perfect blend of culture, nature, and relaxation in this Indonesian gem.",
                    Country = "Indonesia",
                    State = "Bali",
                    City = "Ubud",
                    Latitude = -8.5069f,
                    Longitude = 115.2625f,
                    Category = "Beach & Culture",
                    Tags = new List<string> { "beach", "temples", "culture", "nature", "relaxation" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=800" },
                    Rating = 4.8f,
                    ReviewCount = 1247,
                    Categories = new List<string> { "Beach", "Culture", "Nature" },
                    BestSeasons = new List<string> { "Apr-Oct" },
                    RecommendedDuration = 7,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Place
                {
                    Name = "Swiss Alps Adventure",
                    Description = "Breathtaking mountain landscapes, pristine lakes, and charming villages. Perfect for hiking, skiing, and experiencing authentic Alpine culture.",
                    Country = "Switzerland",
                    State = "Valais",
                    City = "Zermatt",
                    Latitude = 46.0207f,
                    Longitude = 7.7491f,
                    Category = "Mountain & Adventure",
                    Tags = new List<string> { "mountains", "skiing", "hiking", "nature", "adventure" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800" },
                    Rating = 4.9f,
                    ReviewCount = 892,
                    Categories = new List<string> { "Mountain", "Adventure", "Nature" },
                    BestSeasons = new List<string> { "Dec-Mar", "Jun-Sep" },
                    RecommendedDuration = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Place
                {
                    Name = "Tokyo Cultural Experience",
                    Description = "Ultra-modern metropolis blending traditional culture with cutting-edge technology. Discover ancient temples, world-class cuisine, and vibrant neighborhoods.",
                    Country = "Japan",
                    State = "Tokyo",
                    City = "Tokyo",
                    Latitude = 35.6762f,
                    Longitude = 139.6503f,
                    Category = "City & Culture",
                    Tags = new List<string> { "culture", "food", "technology", "temples", "urban" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800" },
                    Rating = 4.7f,
                    ReviewCount = 2156,
                    Categories = new List<string> { "Culture", "City", "Food" },
                    BestSeasons = new List<string> { "Mar-May", "Sep-Nov" },
                    RecommendedDuration = 8,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Place
                {
                    Name = "Maldives Luxury Resort",
                    Description = "Crystal-clear waters, pristine white sand beaches, and overwater bungalows. The ultimate tropical luxury experience for honeymoons and romantic getaways.",
                    Country = "Maldives",
                    State = "Malé",
                    City = "Malé",
                    Latitude = 4.1755f,
                    Longitude = 73.5093f,
                    Category = "Beach & Luxury",
                    Tags = new List<string> { "beach", "luxury", "romance", "diving", "resort" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1514282401047-d79a71a590e8?w=800" },
                    Rating = 4.9f,
                    ReviewCount = 567,
                    Categories = new List<string> { "Beach", "Luxury", "Romance" },
                    BestSeasons = new List<string> { "Nov-Apr" },
                    RecommendedDuration = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Places.AddRangeAsync(places);
        }

        private async Task SeedPackageProvidersAsync()
        {
            var providers = new List<PackageProvider>
            {
                new PackageProvider
                {
                    CompanyName = "Adventure World Tours",
                    Description = "Specializing in adventure and cultural tours worldwide with 20+ years of experience.",
                    ContactPersonName = "Sarah Johnson",
                    ContactEmail = "sarah@adventureworld.com",
                    ContactPhone = "+1-555-0123",
                    Address = "123 Travel Street, New York, NY 10001",
                    Website = "https://adventureworld.com",
                    LogoUrl = "https://images.unsplash.com/photo-1488646953014-85cb44e25828?w=200",
                    LicenseNumber = "AWT-2024-001",
                    IsVerified = true,
                    IsActive = true,
                    CommissionRate = 0.15m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PackageProvider
                {
                    CompanyName = "Luxury Escapes Ltd",
                    Description = "Premium luxury travel experiences for discerning travelers seeking exceptional service.",
                    ContactPersonName = "Michael Chen",
                    ContactEmail = "michael@luxuryescapes.com",
                    ContactPhone = "+1-555-0456",
                    Address = "456 Luxury Ave, Beverly Hills, CA 90210",
                    Website = "https://luxuryescapes.com",
                    LogoUrl = "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=200",
                    LicenseNumber = "LE-2024-002",
                    IsVerified = true,
                    IsActive = true,
                    CommissionRate = 0.20m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.PackageProviders.AddRangeAsync(providers);
        }

        private async Task SeedHotelsAsync()
        {
            var places = await _context.Places.ToListAsync();
            if (!places.Any()) return;

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Name = "Bali Paradise Resort & Spa",
                    Description = "Luxury beachfront resort with world-class spa facilities, infinity pools, and authentic Balinese architecture.",
                    Address = "Jl. Pantai Paradise, Ubud, Bali 80571",
                    StarRating = 5,
                    PricePerNight = 299.00m,
                    Amenities = new List<string> { "Free WiFi", "Spa", "Pool", "Restaurant", "Beach Access", "Fitness Center" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800" },
                    ContactPhone = "+62-361-123456",
                    ContactEmail = "info@baliparadise.com",
                    Website = "https://baliparadise.com",
                    HasWifi = true,
                    HasParking = true,
                    HasSwimmingPool = true,
                    HasGym = true,
                    HasSpa = true,
                    PetFriendly = false,
                    TotalRooms = 150,
                    IsActive = true,
                    PlaceId = places.First(p => p.Name == "Bali Paradise").Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Name = "Alpine Grand Hotel",
                    Description = "Historic mountain hotel with stunning Alpine views, traditional Swiss hospitality, and modern luxury amenities.",
                    Address = "Bahnhofstrasse 1, 3920 Zermatt, Switzerland",
                    StarRating = 4,
                    PricePerNight = 450.00m,
                    Amenities = new List<string> { "Free WiFi", "Restaurant", "Bar", "Ski Storage", "Mountain Views", "Concierge" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800" },
                    ContactPhone = "+41-27-966-2600",
                    ContactEmail = "reservations@alpinegrand.ch",
                    Website = "https://alpinegrand.ch",
                    HasWifi = true,
                    HasParking = true,
                    HasSwimmingPool = false,
                    HasGym = true,
                    HasSpa = true,
                    PetFriendly = true,
                    TotalRooms = 89,
                    IsActive = true,
                    PlaceId = places.First(p => p.Name == "Swiss Alps Adventure").Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Hotels.AddRangeAsync(hotels);
        }

        private async Task SeedTourPackagesAsync()
        {
            var places = await _context.Places.ToListAsync();
            var providers = await _context.PackageProviders.ToListAsync();
            
            if (!places.Any() || !providers.Any()) return;

            var packages = new List<TourPackage>
            {
                new TourPackage
                {
                    Name = "Bali Cultural & Beach Escape",
                    Description = "7-day journey through Bali's cultural heart and pristine beaches. Visit ancient temples, explore traditional villages, enjoy spa treatments, and relax on world-famous beaches.",
                    Price = 899.00m,
                    Duration = 7,
                    Category = "Culture & Beach",
                    Difficulty = "Easy",
                    MaxGroupSize = 16,
                    Inclusions = new List<string> { "Accommodation", "Daily Breakfast", "Cultural Tours", "Temple Visits", "Spa Treatment", "Airport Transfers" },
                    Exclusions = new List<string> { "International Flights", "Lunch & Dinner", "Personal Expenses", "Travel Insurance" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=800" },
                    Itinerary = string.Join("\n", new string[]
                    { 
                        "Day 1: Arrival in Bali, transfer to Ubud",
                        "Day 2: Temple tours and traditional village visit",
                        "Day 3: Rice terrace trekking and cooking class",
                        "Day 4: Transfer to beach resort, spa treatment",
                        "Day 5-6: Beach relaxation and water activities",
                        "Day 7: Departure"
                    }),
                    IsActive = true,
                    PlaceId = places.First(p => p.Name == "Bali Paradise").Id,
                    ProviderId = providers.First(p => p.CompanyName == "Adventure World Tours").Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TourPackage
                {
                    Name = "Swiss Alps Luxury Adventure",
                    Description = "10-day premium Alpine experience with luxury accommodations, private transfers, and exclusive mountain experiences including helicopter tours and Michelin-starred dining.",
                    Price = 2499.00m,
                    Duration = 10,
                    Category = "Luxury Adventure",
                    Difficulty = "Moderate",
                    MaxGroupSize = 8,
                    Inclusions = new List<string> { "Luxury Accommodation", "All Meals", "Private Transfers", "Helicopter Tour", "Mountain Guides", "Equipment Rental" },
                    Exclusions = new List<string> { "International Flights", "Personal Expenses", "Travel Insurance", "Gratuities" },
                    ImageUrls = new List<string> { "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800" },
                    Itinerary = string.Join("\n", new string[]
                    { 
                        "Day 1-2: Arrival in Zurich, transfer to Zermatt",
                        "Day 3-4: Matterhorn glacier tours and hiking",
                        "Day 5-6: Interlaken adventure activities",
                        "Day 7-8: St. Moritz luxury experience",
                        "Day 9: Helicopter tour and farewell dinner",
                        "Day 10: Departure from Zurich"
                    }),
                    IsActive = true,
                    PlaceId = places.First(p => p.Name == "Swiss Alps Adventure").Id,
                    ProviderId = providers.First(p => p.CompanyName == "Luxury Escapes Ltd").Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.TourPackages.AddRangeAsync(packages);
        }
    }
}

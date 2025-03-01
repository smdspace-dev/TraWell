using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraWell.Data;
using System.Reflection;
using System.Diagnostics;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TraWellDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TraWellDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                var healthCheck = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Checks = new
                    {
                        Database = await CheckDatabaseHealth(),
                        Application = CheckApplicationHealth()
                    }
                };

                return Ok(healthCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = "Service unavailable"
                });
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                // Check if database is accessible
                await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Error = "Database not accessible"
                });
            }
        }

        [HttpGet("live")]
        public IActionResult GetLiveness()
        {
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow
            });
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return new { Status = "Unhealthy", Message = "Cannot connect to database" };
                }

                // Check table counts
                var placeCount = await _context.Places.CountAsync();
                var packageCount = await _context.TourPackages.CountAsync();
                var hotelCount = await _context.Hotels.CountAsync();

                return new
                {
                    Status = "Healthy",
                    ConnectionStatus = "Connected",
                    Tables = new
                    {
                        Places = placeCount,
                        TourPackages = packageCount,
                        Hotels = hotelCount
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Unhealthy",
                    Message = ex.Message
                };
            }
        }

        private object CheckApplicationHealth()
        {
            try
            {
                var memoryUsage = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;

                return new
                {
                    Status = "Healthy",
                    Memory = new
                    {
                        GCMemory = $"{memoryUsage / 1024 / 1024} MB",
                        WorkingSet = $"{workingSet / 1024 / 1024} MB"
                    },
                    Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Unhealthy",
                    Message = ex.Message
                };
            }
        }
    }
}

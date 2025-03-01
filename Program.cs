using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TraWell.Data;
using TraWell.Models;
using TraWell.Services;
using TraWell.Middleware;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = new[]
    {
        "text/plain",
        "text/html",
        "text/css",
        "text/javascript",
        "application/javascript",
        "application/json",
        "application/xml",
        "text/xml",
        "image/svg+xml"
    };
});

// Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
    options.AddPolicy("StaticFiles", policy => policy.Expire(TimeSpan.FromHours(24)));
    options.AddPolicy("ApiData", policy => policy.Expire(TimeSpan.FromMinutes(5)));
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                      "Data Source=TraWell.db";

// Configure database provider based on environment
if (builder.Environment.IsEnvironment("Testing"))
{
    // Use in-memory database for testing (will be overridden in test factory)
    builder.Services.AddDbContext<TraWellDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase"));
}
else if (builder.Environment.IsProduction())
{
    // Use PostgreSQL for production
    builder.Services.AddDbContext<TraWellDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use SQLite for development
    builder.Services.AddDbContext<TraWellDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TraWellDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "TraWell2024SecretKeyForJWTTokenGeneration");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Add HttpClient service
builder.Services.AddHttpClient();

// Register Custom Services
builder.Services.AddScoped<PlaceService>();
builder.Services.AddScoped<VideoService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IPaymentService, RazorpayService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddMemoryCache(); // Required for OTP service
builder.Services.AddHostedService<BackgroundVideoScraper>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("TraWellCorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow localhost origins
            policy.WithOrigins("http://localhost:3000", "http://localhost:5088", "https://localhost:5089")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Production: Add your specific domains
            policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseResponseCompression(); // Add compression early in pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();
app.UseOutputCache(); // Add output caching
// Temporarily disable rate limiting for development
// app.UseMiddleware<RateLimitingMiddleware>();
app.UseCors("TraWellCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

// Ensure default roles exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    string[] roleNames = { "Admin", "User", "Provider" };
    
    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default admin user
    var adminEmail = "admin@trawell.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "TraWell",
            DateOfBirth = new DateTime(1990, 1, 1),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow,
            Bio = "System Administrator",
            ProfileImageUrl = ""
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("✅ Default admin user created:");
            Console.WriteLine($"   Email: {adminEmail}");
            Console.WriteLine($"   Password: Admin@123");
        }
    }
}

// Create database if it doesn't exist and seed sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TraWellDbContext>();
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    
    try
    {
        context.Database.EnsureCreated();
        await dataSeeder.SeedSampleDataAsync();
        Console.WriteLine("✅ Database and sample data initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Database initialization failed: {ex.Message}");
        // Continue with application startup
    }
}

// Auto-open browser in development
if (app.Environment.IsDevelopment())
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var address = app.Urls.FirstOrDefault() ?? "http://localhost:5088";
            Console.WriteLine($"\n🌐 Opening browser at: {address}");
            
            // Open browser cross-platform
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {address}") { CreateNoWindow = true });
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                Process.Start("open", address);
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Process.Start("xdg-open", address);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not auto-open browser: {ex.Message}");
        }
    });
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }

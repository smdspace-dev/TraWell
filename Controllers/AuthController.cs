using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TraWell.Models;
using TraWell.Models.DTOs;
using TraWell.Services;

namespace TraWell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IEmailService emailService,
            IOtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _otpService = otpService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("=== REGISTRATION ATTEMPT START ===");
                _logger.LogInformation("Raw request received. Content-Type: {ContentType}", Request.ContentType);
                
                // Basic validation
                if (request == null)
                {
                    _logger.LogError("Request is null - deserialization failed");
                    return BadRequest(new { message = "Invalid request data - please check JSON format" });
                }
                
                _logger.LogInformation("Request properties - Email: '{Email}', FirstName: '{FirstName}', LastName: '{LastName}', Password: '{PasswordLength} chars'", 
                    request.Email ?? "NULL", 
                    request.FirstName ?? "NULL", 
                    request.LastName ?? "NULL",
                    request.Password?.Length ?? 0);
                
                _logger.LogInformation("Email: {Email}", request.Email);
                _logger.LogInformation("First Name: {FirstName}", request.FirstName);
                
                // Check if email is valid
                if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                {
                    return BadRequest(new { message = "Valid email is required" });
                }
                
                // Check if password is provided
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Password is required" });
                }
                
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User already exists: {Email}", request.Email);
                    return BadRequest(new { message = "User already exists with this email" });
                }
                
                // Generate and send OTP instead of creating user immediately
                var otp = _otpService.GenerateOtp();
                await _otpService.StoreOtpAsync(request.Email, otp);
                
                _logger.LogInformation("OTP generated for: {Email}", request.Email);
                
                // Send OTP via email
                try
                {
                    await _emailService.SendOtpEmailAsync(
                        request.Email, 
                        otp,
                        request.FirstName ?? "User"
                    );
                    
                    _logger.LogInformation("OTP sent successfully to: {Email}", request.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send OTP email to: {Email}", request.Email);
                    // Still proceed - user can try again
                }
                
                // Store registration data temporarily (you might want to use a cache or database for this)
                // For now, we'll expect the frontend to send the data again with OTP
                
                _logger.LogInformation("Registration OTP flow initiated for: {Email}", request.Email);
                
                return Ok(new
                {
                    requiresOtp = true,
                    message = "Verification code sent to your email. Please check your inbox.",
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for: {Email}", request?.Email);
                return StatusCode(500, new { message = "Server error during registration" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var isValidOtp = await _otpService.VerifyOtpAsync(request.Email, request.Otp);
                if (!isValidOtp)
                {
                    return BadRequest(new { message = "Invalid or expired OTP" });
                }

                // Create the user after OTP verification
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    // Assign default "User" role
                    await _userManager.AddToRoleAsync(user, "User");

                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);

                    var token = await GenerateJwtToken(user);
                    var userDto = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber ?? "",
                        Roles = new List<string> { "User" }
                    };

                    return Ok(new AuthResponse
                    {
                        Token = token.Token,
                        Expiry = token.Expiry,
                        User = userDto
                    });
                }

                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP verification");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            try
            {
                // Generate and send new OTP
                var otp = _otpService.GenerateOtp();
                await _otpService.StoreOtpAsync(request.Email, otp);
                await _emailService.SendOtpEmailAsync(request.Email, otp, "User");

                return Ok(new { message = "New OTP sent to your email" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP");
                return StatusCode(500, new { message = "Failed to resend OTP" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var token = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);
                
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.ToList()
                };

                // Update last active time
                user.LastActive = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new AuthResponse
                {
                    Token = token.Token,
                    Expiry = token.Expiry,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.ToList()
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                
                if (result.Succeeded)
                {
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private async Task<(string Token, DateTime Expiry)> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "TraWell2024SecretKeyForJWTTokenGeneration");
            
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"] ?? "1440")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return (tokenHandler.WriteToken(token), tokenDescriptor.Expires.Value);
        }
    }
}

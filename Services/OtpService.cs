using Microsoft.Extensions.Caching.Memory;

namespace TraWell.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> StoreOtpAsync(string email, string otp);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task InvalidateOtpAsync(string email);
    }

    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(10);

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public string GenerateOtp()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            _logger.LogInformation("Generated OTP: {Otp}", otp);
            return otp;
        }

        public Task<bool> StoreOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = GetOtpCacheKey(email);
                var otpData = new OtpData
                {
                    Code = otp,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    AttemptCount = 0
                };

                _cache.Set(cacheKey, otpData, _otpExpiry);
                _logger.LogInformation("Stored OTP for email: {Email}", email);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store OTP for email: {Email}", email);
                return Task.FromResult(false);
            }
        }

        public Task<bool> VerifyOtpAsync(string email, string otp)
        {
            try
            {
                var cacheKey = GetOtpCacheKey(email);
                
                if (!_cache.TryGetValue(cacheKey, out OtpData? otpData) || otpData == null)
                {
                    _logger.LogWarning("No OTP found for email: {Email}", email);
                    return Task.FromResult(false);
                }

                // Check attempt count (max 3 attempts)
                if (otpData.AttemptCount >= 3)
                {
                    _logger.LogWarning("Too many OTP attempts for email: {Email}", email);
                    _cache.Remove(cacheKey);
                    return Task.FromResult(false);
                }

                // Increment attempt count
                otpData.AttemptCount++;
                _cache.Set(cacheKey, otpData, _otpExpiry);

                // Verify OTP
                if (otpData.Code == otp)
                {
                    _logger.LogInformation("OTP verified successfully for email: {Email}", email);
                    _cache.Remove(cacheKey); // Remove OTP after successful verification
                    return Task.FromResult(true);
                }

                _logger.LogWarning("Invalid OTP provided for email: {Email}", email);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify OTP for email: {Email}", email);
                return Task.FromResult(false);
            }
        }

        public Task InvalidateOtpAsync(string email)
        {
            try
            {
                var cacheKey = GetOtpCacheKey(email);
                _cache.Remove(cacheKey);
                _logger.LogInformation("Invalidated OTP for email: {Email}", email);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate OTP for email: {Email}", email);
                return Task.CompletedTask;
            }
        }

        private static string GetOtpCacheKey(string email)
        {
            return $"otp:{email.ToLowerInvariant()}";
        }

        private class OtpData
        {
            public string Code { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public int AttemptCount { get; set; }
        }
    }
}

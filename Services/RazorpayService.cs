using System.Text.Json;

namespace TraWell.Services
{
    public interface IPaymentService
    {
        Task<PaymentOrderResponse> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, object>? metadata = null);
        Task<bool> VerifyPaymentAsync(string paymentId, string orderId, string signature);
        Task<PaymentDetailsResponse> GetPaymentDetailsAsync(string paymentId);
    }

    public class RazorpayService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpayService> _logger;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(HttpClient httpClient, IConfiguration configuration, ILogger<RazorpayService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _keyId = _configuration["RazorpaySettings:KeyId"] ?? throw new ArgumentNullException("RazorpaySettings:KeyId");
            _keySecret = _configuration["RazorpaySettings:KeySecret"] ?? throw new ArgumentNullException("RazorpaySettings:KeySecret");

            // Setup basic auth header
            var authValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            _httpClient.BaseAddress = new Uri(_configuration["RazorpaySettings:BaseUrl"] ?? "https://api.razorpay.com/v1/");
        }

        public async Task<PaymentOrderResponse> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var orderData = new
                {
                    amount = (int)(amount * 100), // Razorpay expects amount in paisa
                    currency = currency.ToUpper(),
                    receipt = receipt,
                    notes = metadata ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(orderData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("orders", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var orderResponse = JsonSerializer.Deserialize<PaymentOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    _logger.LogInformation("Order created successfully: {OrderId}", orderResponse?.Id);
                    return orderResponse ?? throw new InvalidOperationException("Failed to deserialize order response");
                }
                else
                {
                    _logger.LogError("Failed to create order: {Response}", responseContent);
                    throw new Exception($"Failed to create payment order: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order");
                throw;
            }
        }

        public Task<bool> VerifyPaymentAsync(string paymentId, string orderId, string signature)
        {
            try
            {
                var body = orderId + "|" + paymentId;
                var secret = _keySecret;

                using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(body));
                var computedSignature = Convert.ToHexString(computedHash).ToLower();

                var isValid = computedSignature == signature.ToLower();
                _logger.LogInformation("Payment verification result: {IsValid} for payment {PaymentId}", isValid, paymentId);

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment signature");
                return Task.FromResult(false);
            }
        }

        public async Task<PaymentDetailsResponse> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"payments/{paymentId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var paymentDetails = JsonSerializer.Deserialize<PaymentDetailsResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    return paymentDetails ?? throw new InvalidOperationException("Failed to deserialize payment details");
                }
                else
                {
                    _logger.LogError("Failed to get payment details: {Response}", responseContent);
                    throw new Exception($"Failed to get payment details: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment details for {PaymentId}", paymentId);
                throw;
            }
        }
    }

    // Response DTOs
    public class PaymentOrderResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int AmountPaid { get; set; }
        public int AmountDue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
        public string OfferId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public Dictionary<string, object> Notes { get; set; } = new();
        public long CreatedAt { get; set; }
    }

    public class PaymentDetailsResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string InvoiceId { get; set; } = string.Empty;
        public bool International { get; set; }
        public string Method { get; set; } = string.Empty;
        public int AmountRefunded { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public bool Captured { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Wallet { get; set; } = string.Empty;
        public string Vpa { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public Dictionary<string, object> Notes { get; set; } = new();
        public int Fee { get; set; }
        public int Tax { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public string ErrorSource { get; set; } = string.Empty;
        public string ErrorStep { get; set; } = string.Empty;
        public string ErrorReason { get; set; } = string.Empty;
        public long CreatedAt { get; set; }
    }
}

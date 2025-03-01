using System.Collections.Concurrent;

namespace TraWell.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private readonly int _maxRequests = 100;
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var now = DateTime.UtcNow;

            var clientInfo = _clients.GetOrAdd(clientId, _ => new ClientRequestInfo());
            
            bool rateLimitExceeded = false;
            int remainingRequests = 0;

            lock (clientInfo)
            {
                // Clean up old requests
                clientInfo.RequestTimes.RemoveAll(time => now - time > _timeWindow);

                // Check rate limit
                if (clientInfo.RequestTimes.Count >= _maxRequests)
                {
                    rateLimitExceeded = true;
                }
                else
                {
                    // Add current request
                    clientInfo.RequestTimes.Add(now);
                    remainingRequests = _maxRequests - clientInfo.RequestTimes.Count;
                }
            }

            if (rateLimitExceeded)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)now.Add(_timeWindow)).ToUnixTimeSeconds().ToString();
                
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remainingRequests.ToString();

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as identifier
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            return clientIp ?? "unknown";
        }

        private class ClientRequestInfo
        {
            public List<DateTime> RequestTimes { get; set; } = new List<DateTime>();
        }
    }
}

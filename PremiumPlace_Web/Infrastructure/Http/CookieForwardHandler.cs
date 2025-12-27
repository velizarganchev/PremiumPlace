namespace PremiumPlace_Web.Infrastructure.Http
{
    /// <summary>
    /// Forwards auth cookies from the incoming browser request (MVC) to the outgoing API request (HttpClient).
    /// This enables BFF mode where the browser only talks to MVC, and MVC talks to the API.
    /// </summary>
    public sealed class CookieForwardHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Only forward the cookies we actually need.
        private static readonly string[] ForwardCookieNames =
        [
            "pp_access",
            "pp_refresh"
        ];

        public CookieForwardHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return base.SendAsync(request, cancellationToken);

            // Build a Cookie header like: "pp_access=...; pp_refresh=..."
            var parts = new List<string>(capacity: ForwardCookieNames.Length);

            foreach (var name in ForwardCookieNames)
            {
                if (httpContext.Request.Cookies.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    // NOTE: Cookie header expects "name=value" pairs separated by "; "
                    parts.Add($"{name}={value}");
                }
            }

            if (parts.Count > 0)
            {
                // Avoid duplicates if the caller already set Cookie header.
                request.Headers.Remove("Cookie");
                request.Headers.Add("Cookie", string.Join("; ", parts));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PremiumPlace.DTO.Auth;
using PremiumPlace_Web.Infrastructure.Http;

namespace PremiumPlace_Web.Controllers
{
    [ApiController]
    [Route("bff/auth")]
    public class BffAuthController : ControllerBase
    {
        private readonly PremiumPlaceApiClient _api;

        public BffAuthController(PremiumPlaceApiClient api)
        {
            _api = api;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto, CancellationToken ct)
        {
            // 1) MVC calls API login
            var apiResp = await _api.PostJsonAsync("/api/auth/login", dto, ct);

            // 2) Forward Set-Cookie from API to browser (so cookies are stored for MVC origin)
            SetCookieForwarding.CopySetCookieHeaders(apiResp, Response);

            return await Proxy(apiResp, ct);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto, CancellationToken ct)
        {
            var apiResp = await _api.PostJsonAsync("/api/auth/register", dto, ct);
            SetCookieForwarding.CopySetCookieHeaders(apiResp, Response);

            return await Proxy(apiResp, ct);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var apiResp = await _api.PostJsonAsync("/api/auth/logout", new { }, ct);
            SetCookieForwarding.CopySetCookieHeaders(apiResp, Response);

            return await Proxy(apiResp, ct);
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            // Cookies are forwarded automatically by CookieForwardHandler.
            // If access token expired, RefreshOn401Handler will refresh and retry.
            var apiResp = await _api.GetAsync("/api/auth/me", ct);

            return await Proxy(apiResp, ct);
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe([FromBody] DeleteMeRequestDTO dto, CancellationToken ct)
        {
            // Same flow as Angular interceptor: cookies forward + refresh+retry happens server-side.
            var apiResp = await _api.DeleteJsonAsync("/api/auth/me", dto, ct);
            SetCookieForwarding.CopySetCookieHeaders(apiResp, Response);

            return await Proxy(apiResp, ct);
        }

        private static async Task<ContentResult> Proxy(HttpResponseMessage apiResp, CancellationToken ct)
        {
            var contentType = apiResp.Content.Headers.ContentType?.ToString() ?? "application/json";
            var body = await apiResp.Content.ReadAsStringAsync(ct);

            return new ContentResult
            {
                StatusCode = (int)apiResp.StatusCode,
                ContentType = contentType,
                Content = body
            };
        }

    }
}

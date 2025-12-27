using Microsoft.AspNetCore.Mvc;
using PremiumPlace.DTO.Auth;
using PremiumPlace_Web.Infrastructure.Http;
using System.Text.Json;

namespace PremiumPlace_Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly PremiumPlaceApiClient _api;
        public AuthController(PremiumPlaceApiClient api)
        {
            _api = api;
        }

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO dto, CancellationToken ct)
        {
            // 1) Client-side validation (DTO Required)
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required fields.";
                return View(dto);
            }

            var apiResp = await _api.PostJsonAsync("/api/auth/login", dto, ct);

            // Forward Set-Cookie (pp_access, pp_refresh) from API -> browser (MVC origin)
            SetCookieForwarding.CopySetCookieHeaders(apiResp, Response);

            if (apiResp.IsSuccessStatusCode)
                return RedirectToAction("Index", "Home");

            // 2) API error -> show in your bootstrap alert
            var apiMessage = await TryExtractMessageAsync(apiResp, ct)
                            ?? "Invalid credentials. Please try again.";

            TempData["error"] = apiMessage;

            return View(dto);
        }

        [HttpGet]
        public IActionResult Register() => View();


        private static async Task<string?> TryExtractMessageAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(json)) return null;

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    return msg.GetString();

                if (doc.RootElement.ValueKind == JsonValueKind.String)
                    return doc.RootElement.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

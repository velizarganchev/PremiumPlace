using Microsoft.AspNetCore.Mvc;
using PremiumPlace.DTO.Auth;
using PremiumPlace_Web.Application.Abstractions.Api;
using PremiumPlace_Web.Infrastructure.Http;
using System.Text.Json;

namespace PremiumPlace_Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IAuthApi _auth;
        public AuthController(IAuthApi auth)
        {
            _auth = auth;
        }

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required fields.";
                return View(dto);
            }

            var result = await _auth.LoginAsync(dto, ct);
            if (result.Success)
                return RedirectToAction("Index", "Home");

            TempData["error"] = result.Message ?? "Invalid credentials. Please try again.";
            return View(dto);
        }

        [HttpGet("register")]
        public IActionResult Register() => View();

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDTO dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required fields.";
                return View(dto);
            }
            var result = await _auth.RegisterAsync(dto, ct);
            if (result.Success)
                return RedirectToAction("Index", "Home");
            TempData["error"] = result.Message ?? "Registration failed. Please try again.";
            return View(dto);
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            await _auth.LogoutAsync(ct);
            return RedirectToAction("Login");
        }
    }
}

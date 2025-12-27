using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PremiumPlace.DTO.Auth;
using PremiumPlace_API.Controllers.Extensions;
using PremiumPlace_API.Controllers.Helpers;
using PremiumPlace_API.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PremiumPlace_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtOptions _jwt;
        public AuthController(IAuthService authService, IOptions<JwtOptions> jwt)
        {
            _authService = authService;
            _jwt = jwt.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto)
        {
            var ip = GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            var sr = await _authService.RegisterAsync(dto, ip, ua);

            if (!sr.Success)
                return this.ToActionResult(sr);

            var result = sr.Data!;

            AuthCookieHelper.SetAuthCookies(Response, result.AccessToken, result.RefreshToken, _jwt.AccessTokenMinutes, _jwt.RefreshTokenDays);

            return Ok(new AuthResponseDTO { User = result.User });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var ip = GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            var sr = await _authService.LoginAsync(dto, ip, ua);
            if (!sr.Success)
                return this.ToActionResult(sr);

            var result = sr.Data!;

            AuthCookieHelper.SetAuthCookies(
                Response,
                result.AccessToken,
                result.RefreshToken,
                _jwt.AccessTokenMinutes,
                _jwt.RefreshTokenDays);

            return Ok(new AuthResponseDTO { User = result.User });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var ip = GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            var refreshToken = Request.Cookies[AuthCookieHelper.RefreshCookieName];

            var sr = await _authService.RefreshAsync(refreshToken ?? "", ip, ua);
            if (!sr.Success)
                return this.ToActionResult(sr);

            var result = sr.Data!;

            AuthCookieHelper.SetAuthCookies(
                Response,
                result.AccessToken,
                result.RefreshToken,
                _jwt.AccessTokenMinutes,
                _jwt.RefreshTokenDays);

            return Ok(new AuthResponseDTO { User = result.User });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var ip = GetClientIp();
            var refreshToken = Request.Cookies[AuthCookieHelper.RefreshCookieName] ?? "";

            await _authService.LogoutAsync(refreshToken, ip);

            AuthCookieHelper.ClearAuthCookies(Response);

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            return Ok(new AuthResponseDTO
            {
                User = new AuthUserDTO
                {
                    Id = int.TryParse(userIdStr, out var id) ? id : 0,
                    Email = email ?? "",
                    Username = username,
                    Role = role
                }
            });
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMe(DeleteMeRequestDTO dto)
        {
            var userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var ip = GetClientIp();

            Console.WriteLine("DeleteMe called. sub=" + User.FindFirstValue("sub"));
            Console.WriteLine("nameid=" + User.FindFirstValue(ClaimTypes.NameIdentifier));


            var sr = await _authService.DeleteMeAsync(userId, dto.Password, ip);
            if (!sr.Success)
                return this.ToActionResult(sr);

            AuthCookieHelper.ClearAuthCookies(Response);
            return NoContent();
        }

        private string GetClientIp()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

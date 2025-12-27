using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PremiumPlace.DTO.Auth;
using PremiumPlace_API.Data;
using PremiumPlace_API.Models;
using PremiumPlace_API.Services.Extensions;

namespace PremiumPlace_API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            ApplicationDbContext db,
            ITokenService tokenService,
            IPasswordService passwordService,
            IOptions<JwtOptions> jwtOptions)
        {
            _db = db;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _jwtOptions = jwtOptions.Value;
        }
        public async Task<ServiceResponse<AuthResultDTO>> LoginAsync(LoginRequestDTO dto, string ip, string? userAgent)
        {
            var key = dto.UsernameOrEmail.Trim().ToLower();
            var user = await _db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == key || u.Username == key);

            if (user == null || !_passwordService.Verify(user, user.PasswordHash, dto.Password))
                {
                return ServiceResponseFactory.Fail<AuthResultDTO>(ServiceErrorType.Unauthorized,
                    "Invalid credentials.");
            }

            var refreshPlain = _tokenService.CreateRefreshToken();
            var refreshHash = _tokenService.HashToken(refreshPlain);

            user.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = refreshHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                CreatedByIp = ip
            });

            CleanupOldRefreshTokens(user);

            await _db.SaveChangesAsync();

            var access = _tokenService.CreateAccessToken(user);

            return ServiceResponseFactory.Ok(new AuthResultDTO
            {
                User = MyAuthMapper.ToAuthUserDto(user),
                AccessToken = access,
                RefreshToken = refreshPlain
            });
        }

        public async Task<ServiceResponse<bool>> LogoutAsync(string refreshToken, string ip)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ServiceResponseFactory.Ok(true);

            var hash = _tokenService.HashToken(refreshToken);

            var user = await _db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == hash));

            if (user is null)
                return ServiceResponseFactory.Ok(true);

            var token = user.RefreshTokens.Single(rt => rt.TokenHash == hash);

            if (token.RevokedAtUtc is null)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
                token.RevokedByIp = ip;
                await _db.SaveChangesAsync();
            }

            return ServiceResponseFactory.Ok(true);
        }

        public async Task<ServiceResponse<AuthResultDTO>> RefreshAsync(string refreshToken, string ip, string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return ServiceResponseFactory.Fail<AuthResultDTO>(
                    ServiceErrorType.Unauthorized,
                    "Missing refresh token.");
            }

            var hash = _tokenService.HashToken(refreshToken);

            var user = await _db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == hash));

            if (user is null)
            {
                return ServiceResponseFactory.Fail<AuthResultDTO>(
                    ServiceErrorType.Unauthorized,
                    "Invalid refresh token.");
            }

            var current = user.RefreshTokens.Single(rt => rt.TokenHash == hash);

            if (!current.IsActive)
            {
                return ServiceResponseFactory.Fail<AuthResultDTO>(
                    ServiceErrorType.Unauthorized,
                    "Refresh token is not active.");
            }

            // rotation: revoke old, add new
            current.RevokedAtUtc = DateTime.UtcNow;
            current.RevokedByIp = ip;

            var newPlain = _tokenService.CreateRefreshToken();
            var newHash = _tokenService.HashToken(newPlain);

            current.ReplacedByTokenHash = newHash;

            user.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = newHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                CreatedByIp = ip
            });

            CleanupOldRefreshTokens(user);

            await _db.SaveChangesAsync();

            var access = _tokenService.CreateAccessToken(user);

            return ServiceResponseFactory.Ok(new AuthResultDTO
            {
                User = MyAuthMapper.ToAuthUserDto(user),
                AccessToken = access,
                RefreshToken = newPlain
            }); 
        }

        public async Task<ServiceResponse<AuthResultDTO>> RegisterAsync(RegisterRequestDTO dto, string ip, string? userAgent)
        {
            var email = dto.Email.Trim().ToLower();
            var username = dto.Username.Trim();

            var existingUserByEmail = await _db.Users
                .AnyAsync(u => u.Email == email || u.Username == username);

            if (existingUserByEmail) {
                return ServiceResponseFactory.Fail<AuthResultDTO>(
                    ServiceErrorType.Conflict,
                    "User with same email or username already exists.");
            }

            var user = new User
            {
                Email= email,
                Username = username,
                Role = UserRole.User,
            };
            user.PasswordHash = _passwordService.Hash(user, dto.Password);

            var refreshPlain = _tokenService.CreateRefreshToken();
            var refreshHash = _tokenService.HashToken(refreshPlain);

            user.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = refreshHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                CreatedByIp = ip,
            });


            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var accessToken = _tokenService.CreateAccessToken(user);

            return ServiceResponseFactory.Ok(new AuthResultDTO
            {
                User = MyAuthMapper.ToAuthUserDto(user),
                AccessToken = accessToken,
                RefreshToken = refreshPlain
            }); 
        }

        public async Task<ServiceResponse<bool>> DeleteMeAsync(int userId, string password, string ip)
        {
            var user = await _db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return ServiceResponseFactory.Fail<bool>(ServiceErrorType.NotFound, "User not found.");

            // confirm password
            if (!_passwordService.Verify(user, user.PasswordHash, password))
                return ServiceResponseFactory.Fail<bool>(ServiceErrorType.Unauthorized, "Invalid password.");

            // revoke/cleanup tokens first (optional)
            foreach (var rt in user.RefreshTokens.Where(t => t.RevokedAtUtc is null))
            {
                rt.RevokedAtUtc = DateTime.UtcNow;
                rt.RevokedByIp = ip;
            }

            // hard delete
            _db.RefreshTokens.RemoveRange(user.RefreshTokens);
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();

            return ServiceResponseFactory.Ok(true);
        }

        private static void CleanupOldRefreshTokens(User user)
        {
            var threshold = DateTime.UtcNow.AddDays(-10);
            user.RefreshTokens.RemoveAll(rt => !rt.IsActive && rt.ExpiresAtUtc < threshold);
        }
    }
}

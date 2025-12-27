using PremiumPlace_API.Models;

namespace PremiumPlace_API.Services.Auth
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
        string HashToken(string token);
    }
}

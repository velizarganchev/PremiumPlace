using PremiumPlace_API.Models;

namespace PremiumPlace_API.Services.Auth
{
    public interface IPasswordService
    {
        string Hash(User user, string password);
        bool Verify(User user, string hashedPassword, string providedPassword);
    }
}

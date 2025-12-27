using Microsoft.AspNetCore.Identity;
using PremiumPlace_API.Models;

namespace PremiumPlace_API.Services.Auth
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();
        public string Hash(User user, string password)
            => _hasher.HashPassword(user, password);

        public bool Verify(User user, string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}

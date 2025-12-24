using PremiumPlace_API.Models;
using PremiumPlace_API.Models.DTO.Auth;

namespace PremiumPlace_API.Services.Auth
{
    public class MyAuthMapper
    {
        public static AuthUserDTO ToAuthUserDto(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}

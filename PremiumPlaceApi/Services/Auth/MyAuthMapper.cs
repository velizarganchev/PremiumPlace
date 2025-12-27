using PremiumPlace.DTO.Auth;
using PremiumPlace_API.Models;

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

using System.ComponentModel.DataAnnotations;

namespace PremiumPlace.DTO.Auth
{
    public record class LoginRequestDTO
    {
        [Required(ErrorMessage = "Please enter your email or username.")]
        public string UsernameOrEmail { get; init; } = default!;

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; init; } = default!;
    }
}

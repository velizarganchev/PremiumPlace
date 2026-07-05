using System.ComponentModel.DataAnnotations;

namespace PremiumPlace.DTO.Auth
{
    public record DeleteMeRequestDTO(
        [property: Required(ErrorMessage = "Please enter your password.")]
        [property: MaxLength(128)]
        string Password);
}

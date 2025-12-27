namespace PremiumPlace.DTO.Auth
{
    public record class AuthResponseDTO
    {
        public AuthUserDTO User { get; init; } = default!;
    }
}

namespace PremiumPlace.DTO.Auth
{
    public record class AuthResultDTO
    {
        public AuthUserDTO User { get; init; } = default!;
        public string AccessToken { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
    }
}

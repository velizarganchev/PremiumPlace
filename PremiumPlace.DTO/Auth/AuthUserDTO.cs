namespace PremiumPlace.DTO.Auth
{
    public record class AuthUserDTO
    {
        public int Id { get; init; }
        public string Username { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string Role { get; init; } = default!;
    }
}

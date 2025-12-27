using System.ComponentModel.DataAnnotations;

namespace PremiumPlace_API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string TokenHash { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        [MaxLength(256)]
        public string? CreatedByIp { get; set; }

        [MaxLength(256)]
        public string? RevokedByIp { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
    }
}

using System.ComponentModel.DataAnnotations;

namespace PremiumPlace_API.Models
{
    public enum UserRole { User = 0, Admin = 1 }
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}

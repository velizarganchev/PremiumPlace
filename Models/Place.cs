using System.ComponentModel.DataAnnotations;

namespace PremiumPlace_API.Models
{
    public class Place
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [MaxLength(1000)]
        public string? Details { get; set; }

        [Range(0, 10000)]
        public decimal Rate { get; set; }

        public int SquareFeet { get; set; }

        public int Occupancy { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}

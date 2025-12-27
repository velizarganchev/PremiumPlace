using System.ComponentModel.DataAnnotations;

namespace PremiumPlace.DTO
{
    public record class PlaceUpdateDTO
    {
        [Required]
        public required int Id { get; init; }
        public required string Name { get; init; } = default!;
        public string? Details { get; init; }
        public decimal Rate { get; init; }
        public int SquareFeet { get; init; }
        public int Occupancy { get; init; }
        public string? ImageUrl { get; init; }
    }
}

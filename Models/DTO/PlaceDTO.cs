using System.ComponentModel.DataAnnotations;

namespace PremiumPlace_API.Models.DTO
{
    public class PlaceDTO
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Details { get; set; }

        public decimal Rate { get; set; }

        public int SquareFeet { get; set; }

        public int Occupancy { get; set; }

        public string? ImageUrl { get; set; }
    }
}

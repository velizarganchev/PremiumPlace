using System.ComponentModel.DataAnnotations;

namespace PremiumPlace.DTO
{
    /// <summary>
    /// Query parameters for the paged/filtered places search endpoint.
    /// </summary>
    public record PlaceQueryDTO
    {
        /// <summary>Free-text term matched against name, city, details and amenities.</summary>
        public string? Search { get; init; }

        /// <summary>Exact city name to filter by.</summary>
        public string? City { get; init; }

        /// <summary>recommended | priceAsc | priceDesc | capacityDesc</summary>
        public string? Sort { get; init; }

        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        [Range(1, 100)]
        public int PageSize { get; init; } = 12;
    }
}

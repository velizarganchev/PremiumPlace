namespace PremiumPlace.DTO
{
    public record PlaceOptionsDTO
    {
        public List<CityDTO> Cities { get; init; } = new();
        public List<AmenityDTO> Amenities { get; init; } = new();
    }
}

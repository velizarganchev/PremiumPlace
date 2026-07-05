namespace PremiumPlace.DTO
{
    public record PlaceDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Details { get; init; }

        public int GuestCapacity { get; init; }

        public decimal Rate { get; init; }

        public int Beds { get; init; }

        public int CheckInHour { get; init; }

        public int CheckOutHour { get; init; }

        public int SquareFeet { get; init; }

        public string? ImageUrl { get; init; }

        public string City { get; init; } = string.Empty;

        public int CityId { get; init; }

        public PlaceFeaturesDTO Features { get; init; } = new();

        public List<string> Amenitys { get; init; } = new();

        public List<int> AmenityIds { get; init; } = new();

        public ReviewSummaryDTO ReviewSummary { get; init; } = new();
    }
}

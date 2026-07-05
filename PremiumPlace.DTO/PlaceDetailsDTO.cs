namespace PremiumPlace.DTO
{
    public sealed record PlaceDetailsDTO : PlaceDTO
    {
        public List<ReviewDTO> Reviews { get; set; } = new();
    }
}

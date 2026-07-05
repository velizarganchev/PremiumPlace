namespace PremiumPlace.DTO
{
    /// <summary>
    /// A single page of results plus the metadata a client needs to paginate.
    /// </summary>
    public record PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}

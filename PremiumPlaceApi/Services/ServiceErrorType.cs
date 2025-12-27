namespace PremiumPlace_API.Services
{
    public enum ServiceErrorType
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        Unauthorized = 4,
        Forbidden = 5
    }
}

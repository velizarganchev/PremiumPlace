namespace PremiumPlace_Web.Application.Results
{
    public sealed record ApiResult<T>(
    bool Success,
    int StatusCode,
    T? Data,
    string? Message,
    IReadOnlyDictionary<string, string[]>? Errors = null)
    {
        public static ApiResult<T> Ok(T data, int statusCode = 200, string? message = null)
            => new(true, statusCode, data, message, null);

        public static ApiResult<T> Fail(int statusCode, string? message = null,
            IReadOnlyDictionary<string, string[]>? errors = null)
            => new(false, statusCode, default, message, errors);
    }

    public sealed record ApiResult(
        bool Success,
        int StatusCode,
        string? Message,
        IReadOnlyDictionary<string, string[]>? Errors = null)
    {
        public static ApiResult Ok(int statusCode = 200, string? message = null)
            => new(true, statusCode, message, null);

        public static ApiResult Fail(int statusCode, string? message = null,
            IReadOnlyDictionary<string, string[]>? errors = null)
            => new(false, statusCode, message, errors);
    }
}

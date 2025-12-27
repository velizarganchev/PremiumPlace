namespace PremiumPlace_API.Services.Extensions
{
    public class ServiceResponseFactory
    {
        public static ServiceResponse<T> Ok<T>(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ServiceResponse<T> Fail<T>(ServiceErrorType type, string message, string? error = null) => new()
        {
            Success = false,
            ErrorType = type,
            Message = message,
            Error = error
        };
    }
}

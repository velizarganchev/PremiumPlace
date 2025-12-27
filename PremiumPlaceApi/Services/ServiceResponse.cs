namespace PremiumPlace_API.Services
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public string? Error { get; set; }
        public ServiceErrorType ErrorType { get; set; } = ServiceErrorType.None;
    }
}

namespace PremiumPlace.DTO.PayPal
{
    /// <summary>
    /// Public payment settings the SPA needs to load the PayPal JS SDK.
    /// The client id is not secret (it ships in the browser SDK URL).
    /// </summary>
    public record PaymentConfigDTO
    {
        public string ClientId { get; init; } = "";
        public string Currency { get; init; } = "EUR";
    }
}

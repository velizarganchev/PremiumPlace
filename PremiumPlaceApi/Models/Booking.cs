namespace PremiumPlace_API.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public Place Place { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Check-in inclusive, check-out exclusive (standard booking model)
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }   // expected total for PayPal validation
        public string CurrencyCode { get; set; } = "EUR"; // ISO currency, e.g. "EUR"

        // Optional: keep a reference to payment/order for demo flow
        public string? PaymentRef { get; set; }
    }

    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Failed = 3,
        Expired = 4
    }
}

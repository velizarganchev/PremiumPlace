using PremiumPlace.DTO.Bookings;

namespace PremiumPlace_API.Services.Bookings
{
    public interface IBookingService
    {
        Task<ServiceResponse<AvailabilityResponse>> GetAvailabilityAsync(int placeId, DateOnly from, DateOnly to);

        Task<ServiceResponse<CreatePendingBookingResult>> CreatePendingBookingAsync(int userId, CreateBookingRequest req);

        Task<ServiceResponse<ConfirmBookingResult>> ConfirmBookingAsync(int userId, ConfirmBookingRequest req);

        Task<ServiceResponse<List<MyBookingDTO>>> GetMyBookingsAsync(int userId);

        Task<ServiceResponse<object>> CancelBookingAsync(int userId, int bookingId);

        /// <summary>Marks pending bookings older than the TTL as Expired. Returns the number affected.</summary>
        Task<int> ExpireStalePendingsAsync();
    }
}

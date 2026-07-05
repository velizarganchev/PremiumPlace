using Microsoft.EntityFrameworkCore;
using PremiumPlace.DTO.Bookings;
using PremiumPlace_API.Data;
using PremiumPlace_API.Infrastructure.Payments.PayPal;
using PremiumPlace_API.Models;
using PremiumPlace_API.Services.PayPal;

namespace PremiumPlace_API.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayPalPaymentVerifier _payPalVerifier;
        private readonly PayPalOptions _payPalOptions;
        private readonly ILogger<BookingService> _logger;

        // Pending bookings not confirmed within this window are considered expired.
        public const int PendingTtlMinutes = 30;

        public BookingService(ApplicationDbContext db, IPayPalPaymentVerifier payPalVerifier,
        Microsoft.Extensions.Options.IOptions<PayPalOptions> payPalOptions,
        ILogger<BookingService> logger)
        {
            _db = db;
            _payPalVerifier = payPalVerifier;
            _payPalOptions = payPalOptions.Value;
            _logger = logger;
        }

        public async Task<ServiceResponse<AvailabilityResponse>> GetAvailabilityAsync(int placeId, DateOnly from, DateOnly to)
        {
            if (placeId <= 0)
                return Fail<AvailabilityResponse>("placeId is required.", ServiceErrorType.Validation);

            if (to <= from)
                return Fail<AvailabilityResponse>("Invalid range: 'to' must be after 'from'.", ServiceErrorType.Validation);

            var blocked = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.PlaceId == placeId && b.Status == BookingStatus.Confirmed)
                .Where(b => b.CheckInDate < to && b.CheckOutDate > from)
                .Select(b => new DateRangeDto
                {
                    From = b.CheckInDate,
                    To = b.CheckOutDate
                })
                .ToListAsync();

            return new ServiceResponse<AvailabilityResponse>
            {
                Success = true,
                Data = new AvailabilityResponse(blocked),
                Message = "Availability retrieved successfully."
            };
        }

        // -----------------------------
        // 1) Create pending booking
        // -----------------------------
        public async Task<ServiceResponse<CreatePendingBookingResult>> CreatePendingBookingAsync(int userId, CreateBookingRequest req)
        {
            if (userId <= 0)
                return Fail<CreatePendingBookingResult>("Unauthorized.", ServiceErrorType.Unauthorized);

            if (req is null)
                return Fail<CreatePendingBookingResult>("Booking data is required.", ServiceErrorType.Validation);

            if (req.PlaceId <= 0)
                return Fail<CreatePendingBookingResult>("PlaceId is required.", ServiceErrorType.Validation);

            if (req.CheckOutDate <= req.CheckInDate)
                return Fail<CreatePendingBookingResult>("Check-out must be after check-in.", ServiceErrorType.Validation);

            var nights = req.CheckOutDate.DayNumber - req.CheckInDate.DayNumber;
            if (nights < 1)
                return Fail<CreatePendingBookingResult>("Minimum stay is 1 night.", ServiceErrorType.Validation);

            var place = await _db.Places.AsNoTracking().Select(p => new { p.Id, p.Name, p.Rate }).FirstOrDefaultAsync(p => p.Id == req.PlaceId);

            if (place is null)
                return Fail<CreatePendingBookingResult>("Place not found.", ServiceErrorType.NotFound);

            // overlap check against CONFIRMED only
            var hasOverlap = await _db.Bookings
                .Where(b => b.PlaceId == req.PlaceId && b.Status == BookingStatus.Confirmed)
                .AnyAsync(b => req.CheckInDate < b.CheckOutDate && req.CheckOutDate > b.CheckInDate);

            if (hasOverlap)
                return Fail<CreatePendingBookingResult>("These dates are no longer available.", ServiceErrorType.Conflict);

            // expected total amount calculated server-side
            var total = decimal.Round(place.Rate * nights, 2); // server truth
            var currency = string.IsNullOrWhiteSpace(_payPalOptions.ExpectedCurrency) ? "EUR" : _payPalOptions.ExpectedCurrency!;

            var booking = new Booking
            {
                PlaceId = req.PlaceId,
                UserId = userId,
                CheckInDate = req.CheckInDate,
                CheckOutDate = req.CheckOutDate,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,

                TotalAmount = total,
                CurrencyCode = currency
            };

            await _db.Bookings.AddAsync(booking);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Booking pending created: BookingId={BookingId}, UserId={UserId}, PlaceId={PlaceId}, Nights={Nights}, Amount={Amount} {Currency}",
                booking.Id, userId, req.PlaceId, nights, total, currency);

            return new ServiceResponse<CreatePendingBookingResult>
            {
                Success = true,
                Data = new CreatePendingBookingResult { BookingId = booking.Id },
                Message = "Pending booking created."
            };
        }

        // -----------------------------
        // 2) Confirm pending booking
        // -----------------------------
        public async Task<ServiceResponse<ConfirmBookingResult>> ConfirmBookingAsync(int userId, ConfirmBookingRequest req)
        {
            if (userId <= 0)
                return Fail<ConfirmBookingResult>("Unauthorized.", ServiceErrorType.Unauthorized);

            if (req is null)
                return Fail<ConfirmBookingResult>("Confirm data is required.", ServiceErrorType.Validation);

            if (req.BookingId <= 0)
                return Fail<ConfirmBookingResult>("Invalid booking ID.", ServiceErrorType.Validation);

            if (string.IsNullOrWhiteSpace(req.PaymentReference))
                return Fail<ConfirmBookingResult>("Payment reference is required.", ServiceErrorType.Validation);

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == req.BookingId && b.UserId == userId);

            if (booking is null)
                return Fail<ConfirmBookingResult>("Booking not found.", ServiceErrorType.NotFound);

            if (booking.Status == BookingStatus.Cancelled)
                return Fail<ConfirmBookingResult>("Booking is cancelled.", ServiceErrorType.Conflict);

            if (booking.Status == BookingStatus.Confirmed)
            {
                return new ServiceResponse<ConfirmBookingResult>
                {
                    Success = true,
                    Data = new ConfirmBookingResult { BookingId = booking.Id, Status = BookingStatus.Confirmed.ToString() },
                    Message = "Booking already confirmed."
                };
            }

            if (booking.Status != BookingStatus.Pending)
                return Fail<ConfirmBookingResult>($"Booking cannot be confirmed from status '{booking.Status}'.", ServiceErrorType.Conflict);

            // Reject confirmation of a reservation that sat pending past its TTL.
            if (booking.CreatedAt < DateTime.UtcNow.AddMinutes(-PendingTtlMinutes))
            {
                booking.Status = BookingStatus.Expired;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Booking confirm rejected (expired): BookingId={BookingId}, UserId={UserId}, CreatedAt={CreatedAt}",
                    booking.Id, userId, booking.CreatedAt);

                return Fail<ConfirmBookingResult>("Reservation has expired. Please start a new booking.", ServiceErrorType.Conflict);
            }

            // Final overlap check (server truth) - protects against race conditions
            var hasOverlap = await _db.Bookings
                .Where(b => b.PlaceId == booking.PlaceId && b.Status == BookingStatus.Confirmed)
                .AnyAsync(b => booking.CheckInDate < b.CheckOutDate && booking.CheckOutDate > b.CheckInDate);

            if (hasOverlap)
            {
                booking.Status = BookingStatus.Failed;
                booking.PaymentRef = req.PaymentReference;
                await _db.SaveChangesAsync();

                _logger.LogWarning(
                    "Booking confirm failed (overlap): BookingId={BookingId}, UserId={UserId}, PlaceId={PlaceId}, PaymentRef={PaymentRef}",
                    booking.Id, userId, booking.PlaceId, req.PaymentReference);

                return Fail<ConfirmBookingResult>("Dates became unavailable before confirmation.", ServiceErrorType.Conflict);
            }

            var idempotencyKey = $"pp-booking-{booking.Id}-{req.PaymentReference}".Replace(" ", "");

            try
            {
                await _payPalVerifier.VerifyOrThrowAsync(
                    payPalOrderId: req.PaymentReference,
                    expectedAmount: booking.TotalAmount,
                    expectedCurrency: booking.CurrencyCode,
                    idempotencyKey: idempotencyKey);
            }
            catch (Exception ex)
            {
                booking.Status = BookingStatus.Failed;
                booking.PaymentRef = req.PaymentReference;
                await _db.SaveChangesAsync();

                _logger.LogWarning(ex,
                    "Booking payment verification failed: BookingId={BookingId}, UserId={UserId}, PaymentRef={PaymentRef}",
                    booking.Id, userId, req.PaymentReference);

                return Fail<ConfirmBookingResult>($"Payment verification failed: {ex.Message}", ServiceErrorType.Conflict);
            }

            booking.Status = BookingStatus.Confirmed;
            booking.PaymentRef = req.PaymentReference;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Booking confirmed: BookingId={BookingId}, UserId={UserId}, PlaceId={PlaceId}, Amount={Amount} {Currency}, PaymentRef={PaymentRef}",
                booking.Id, userId, booking.PlaceId, booking.TotalAmount, booking.CurrencyCode, req.PaymentReference);

            return new ServiceResponse<ConfirmBookingResult>
            {
                Success = true,
                Data = new ConfirmBookingResult { BookingId = booking.Id, Status = BookingStatus.Confirmed.ToString() },
                Message = "Booking confirmed."
            };
        }

        public async Task<ServiceResponse<List<MyBookingDTO>>> GetMyBookingsAsync(int userId)
        {
            if (userId <= 0)
                return Fail<List<MyBookingDTO>>("Unauthorized.", ServiceErrorType.Unauthorized);

            var items = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                // Only real reservations belong in "My reservations"; Pending/Failed/Expired
                // are transient payment states and are hidden.
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Cancelled)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new MyBookingDTO
                {
                    Id = b.Id,
                    PlaceId = b.PlaceId,
                    PlaceName = b.Place.Name,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                    TotalAmount = b.TotalAmount,
                    CurrencyCode = b.CurrencyCode
                })
                .ToListAsync();

            return new ServiceResponse<List<MyBookingDTO>>
            {
                Success = true,
                Data = items,
                Message = "My bookings retrieved successfully."
            };
        }

        public async Task<ServiceResponse<object>> CancelBookingAsync(int userId, int bookingId)
        {
            if (userId <= 0)
                return Fail<object>("Unauthorized.", ServiceErrorType.Unauthorized);

            if (bookingId <= 0)
                return Fail<object>("Invalid booking ID.", ServiceErrorType.Validation);

            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            if (booking is null)
                return Fail<object>("Booking not found.", ServiceErrorType.NotFound);

            if (booking.Status == BookingStatus.Cancelled)
                return new ServiceResponse<object> { Success = true, Message = "Booking already cancelled." };

            // allow cancelling Pending & Confirmed for demo
            var previousStatus = booking.Status;
            booking.Status = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Booking cancelled: BookingId={BookingId}, UserId={UserId}, PreviousStatus={PreviousStatus}",
                booking.Id, userId, previousStatus);

            return new ServiceResponse<object>
            {
                Success = true,
                Message = "Booking cancelled successfully."
            };
        }

        public async Task<int> ExpireStalePendingsAsync()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-PendingTtlMinutes);

            var count = await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < cutoff)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.Expired));

            if (count > 0)
                _logger.LogInformation("Expired {Count} stale pending booking(s).", count);

            return count;
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private static ServiceResponse<T> Fail<T>(string message, ServiceErrorType type)
            => new() { Success = false, Message = message, ErrorType = type };
    }
}

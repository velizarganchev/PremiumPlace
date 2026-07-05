namespace PremiumPlace_API.Services.Bookings
{
    /// <summary>
    /// Periodically marks stale pending bookings as Expired so abandoned
    /// checkout flows don't linger in the system.
    /// </summary>
    public sealed class PendingBookingSweeper : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingBookingSweeper> _logger;

        public PendingBookingSweeper(IServiceScopeFactory scopeFactory, ILogger<PendingBookingSweeper> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var bookings = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        await bookings.ExpireStalePendingsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Pending booking sweep failed.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }
    }
}

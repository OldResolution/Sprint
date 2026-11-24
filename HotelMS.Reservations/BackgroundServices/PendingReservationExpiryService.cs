using HotelMS.Reservations.Data;
using HotelMS.Reservations.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Reservations.BackgroundServices;

public class PendingReservationExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingReservationExpiryService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public PendingReservationExpiryService(IServiceScopeFactory scopeFactory, ILogger<PendingReservationExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingReservationExpiryService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();

                var cutoff = DateTime.UtcNow.AddMinutes(-30);
                var expiredReservations = await db.Reservations
                    .Where(r => r.ReservationStatus == ReservationStatus.PendingPayment && r.CreatedAt <= cutoff)
                    .ToListAsync(stoppingToken);

                if (expiredReservations.Any())
                {
                    foreach (var res in expiredReservations)
                    {
                        res.ReservationStatus = ReservationStatus.Cancelled;
                        _logger.LogInformation("Cancelled expired pending reservation {ReservationCode} (Id: {Id})", res.ReservationCode, res.ReservationId);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred while expiring pending reservations.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("PendingReservationExpiryService stopped.");
    }
}

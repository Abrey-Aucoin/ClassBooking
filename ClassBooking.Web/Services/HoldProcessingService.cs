using ClassBooking.Core;
using ClassBooking.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Services
{
    public class HoldProcessingService(IServiceScopeFactory scopeFactory, ILogger<HoldProcessingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Hold processing failed.");
                }
            }
        }

        private async Task ProcessAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;

            var classes = await db.FitnessClasses
                .Include(c => c.Bookings)
                .Where(c => !c.IsCancelled && c.StartTime > now)
                .ToListAsync(stoppingToken);

            var notifications = new List<Notification>();

            foreach (var fitnessClass in classes)
                notifications.AddRange(fitnessClass.ProcessExpiredHolds(now));

            await db.SaveChangesAsync(stoppingToken);

            foreach (var notification in notifications)
                await notifier.SendAsync(notification);
        }
    }
}
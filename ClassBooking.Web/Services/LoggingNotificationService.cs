using ClassBooking.Core;

namespace ClassBooking.Web.Services
{
    public class LoggingNotificationService(ILogger<LoggingNotificationService> logger) : INotificationService
    {
        public Task SendAsync(Notification notification)
        {
            logger.LogInformation("Email to {Email}: {Subject} | {Body}",
                notification.Email, notification.Subject, notification.Body);

            return Task.CompletedTask;
        }
    }
}
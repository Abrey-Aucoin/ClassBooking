using ClassBooking.Core;

namespace ClassBooking.Web.Services
{
    public interface INotificationService
    {
        Task SendAsync(Notification notification);
    }
}
using ClassBooking.Core;

namespace ClassBooking.Web.Models;

public class ManageBookingViewModel
{
    public int BookingId { get; set; }
    public string ClassTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime? HoldExpiresAt { get; set; }
    public bool CanCancel { get; set; }
    public bool CanClaim { get; set; }
}
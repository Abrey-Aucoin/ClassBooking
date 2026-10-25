namespace ClassBooking.Web.Models;

public class RosterEntryViewModel
{
    public int BookingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? HoldExpiresAt { get; set; }
}
using ClassBooking.Core;

namespace ClassBooking.Web.Models;

public class ClassDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public Category Category { get; set; }
    public DateTime StartTime { get; set; }
    public int Capacity { get; set; }
    public int SpotsLeft { get; set; }
    public int WaitlistCount { get; set; }
    public BookingState State { get; set; }
    public BookingFormViewModel BookingForm { get; set; } = new();
    public FindBookingViewModel FindBooking { get; set; } = new();
}
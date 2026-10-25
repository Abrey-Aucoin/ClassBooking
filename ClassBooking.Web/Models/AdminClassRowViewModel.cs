namespace ClassBooking.Web.Models;

public class AdminClassRowViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int ConfirmedCount { get; set; }
    public int Capacity { get; set; }
    public int WaitlistCount { get; set; }
    public bool IsCancelled { get; set; }
}
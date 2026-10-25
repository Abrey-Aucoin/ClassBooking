namespace ClassBooking.Web.Models;

public class RosterViewModel
{
    public int ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int Capacity { get; set; }
    public bool IsCancelled { get; set; }
    public List<RosterEntryViewModel> Confirmed { get; set; } = [];
    public List<RosterEntryViewModel> Offered { get; set; } = [];
    public List<RosterEntryViewModel> Waitlist { get; set; } = [];
}
namespace ClassBooking.Web.Models;

public class InstructorCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int UpcomingClassCount { get; set; }
}
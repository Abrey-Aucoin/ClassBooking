namespace ClassBooking.Web.Dtos;

public class ClassDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int Capacity { get; set; }
    public int SpotsLeft { get; set; }
    public int WaitlistCount { get; set; }
    public bool IsCancelled { get; set; }
}

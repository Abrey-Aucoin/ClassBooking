using ClassBooking.Core;

namespace ClassBooking.Web.Models;

public class ClassSummaryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Category Category { get; set; }
    public DateTime StartTime { get; set; }
    public int SpotsLeft { get; set; }
    public bool IsFull { get; set; }
}
namespace ClassBooking.Web.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? HoldExpiresAt { get; set; }
}

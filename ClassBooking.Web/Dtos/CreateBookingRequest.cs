using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Dtos;

public class CreateBookingRequest
{
    [Required]
    [MaxLength(100)]
    public string MemberName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string MemberEmail { get; set; } = string.Empty;
}

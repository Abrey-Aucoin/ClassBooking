using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class BookingFormViewModel
{
    public int ClassId { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Your name")]
    public string MemberName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    [Display(Name = "Email address")]
    public string MemberEmail { get; set; } = string.Empty;
}
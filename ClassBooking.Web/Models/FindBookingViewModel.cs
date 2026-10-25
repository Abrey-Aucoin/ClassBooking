using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class FindBookingViewModel
{
    public int ClassId { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;
}
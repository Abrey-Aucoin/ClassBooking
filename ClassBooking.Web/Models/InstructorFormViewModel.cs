using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class InstructorFormViewModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }
}
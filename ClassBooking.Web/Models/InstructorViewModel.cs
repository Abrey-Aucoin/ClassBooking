using ClassBooking.Core;
using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class InstructorViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }

    public Category? Category { get; set; }

    [Display(Name = "From")]
    public DateTime? From { get; set; }

    [Display(Name = "Only show classes with open spots")]
    public bool OnlyOpen { get; set; }

    public List<ClassSummaryViewModel> Classes { get; set; } = [];
}
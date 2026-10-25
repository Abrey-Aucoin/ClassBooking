using ClassBooking.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class ClassFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public Category? Category { get; set; }

    [Required]
    [Display(Name = "Instructor")]
    public int? InstructorId { get; set; }

    [Required]
    [Display(Name = "Start time")]
    public DateTime? StartTime { get; set; }

    [Range(1, 200)]
    public int Capacity { get; set; }

    public List<SelectListItem> Instructors { get; set; } = [];
}
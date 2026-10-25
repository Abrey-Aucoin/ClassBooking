using ClassBooking.Core;
using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Models;

public class AdminClassListViewModel
{
    public Category? Category { get; set; }

    [Display(Name = "Show past and cancelled classes")]
    public bool ShowInactive { get; set; }

    public List<AdminClassRowViewModel> Classes { get; set; } = [];
}
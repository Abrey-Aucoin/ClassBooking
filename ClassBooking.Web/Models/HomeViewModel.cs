using ClassBooking.Core;

namespace ClassBooking.Web.Models;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = [];
    public Category? SelectedCategory { get; set; }
    public List<InstructorCardViewModel> Instructors { get; set; } = [];
}
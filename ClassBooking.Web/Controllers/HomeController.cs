using System.Diagnostics;
using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(Category? category)
    {
        var now = DateTime.UtcNow;

        var instructors = await db.Instructors
            .Where(i => i.FitnessClasses.Any(c =>
                !c.IsCancelled && c.StartTime > now && (category == null || c.Category == category)))
            .OrderBy(i => i.Name)
            .Select(i => new InstructorCardViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Bio = i.Bio,
                UpcomingClassCount = i.FitnessClasses.Count(c =>
                    !c.IsCancelled && c.StartTime > now && (category == null || c.Category == category))
            })
            .ToListAsync();

        var model = new HomeViewModel
        {
            Categories = Enum.GetValues<Category>().ToList(),
            SelectedCategory = category,
            Instructors = instructors
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

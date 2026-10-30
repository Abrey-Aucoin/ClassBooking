using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Helpers;
using ClassBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Controllers;

public class InstructorsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Details(int id, Category? category, DateTime? from, bool onlyOpen)
    {
        var instructor = await db.Instructors.FirstOrDefaultAsync(i => i.Id == id);

        if (instructor is null)
            return NotFound();

        var now = DateTime.UtcNow;

        var query = db.FitnessClasses
            .Include(c => c.Bookings)
            .Where(c => c.InstructorId == id && !c.IsCancelled && c.StartTime > now);

        if (category is not null)
            query = query.Where(c => c.Category == category);

        if (from is not null)
        {
            var fromUtc = StudioTime.ToUtc(from.Value.Date);
            query = query.Where(c => c.StartTime >= fromUtc);
        }

        var classes = await query
            .OrderBy(c => c.StartTime)
            .ToListAsync();

        if (onlyOpen)
            classes = classes.Where(c => c.AvailableSpots > 0).ToList();

        var model = new InstructorViewModel
        {
            Id = instructor.Id,
            Name = instructor.Name,
            Bio = instructor.Bio,
            Category = category,
            From = from,
            OnlyOpen = onlyOpen,
            Classes = classes.Select(c => new ClassSummaryViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Category = c.Category,
                StartTime = StudioTime.ToLocal(c.StartTime),
                SpotsLeft = c.AvailableSpots,
                IsFull = c.AvailableSpots == 0
            }).ToList()
        };

        return View(model);
    }
}

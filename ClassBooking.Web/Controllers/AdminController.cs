using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Helpers;
using ClassBooking.Web.Models;
using ClassBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace ClassBooking.Web.Controllers;

public class AdminController(AppDbContext db, INotificationService notifier) : Controller
{
    public async Task<IActionResult> Index(Category? category, bool showInactive)
    {
        var now = DateTime.UtcNow;
        var query = db.FitnessClasses.AsQueryable();

        if (!showInactive)
            query = query.Where(c => !c.IsCancelled && c.StartTime > now);

        if (category is not null)
            query = query.Where(c => c.Category == category);

        var rows = await query
        .OrderBy(c => c.StartTime)
        .Select(c => new AdminClassRowViewModel
        {
            Id = c.Id,
            Title = c.Title,
            InstructorName = c.Instructor.Name,
            StartTime = StudioTime.ToLocal(c.StartTime),
            ConfirmedCount = c.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            Capacity = c.Capacity,
            WaitlistCount = c.Bookings.Count(b => b.Status == BookingStatus.Waitlisted),
            IsCancelled = c.IsCancelled
        })
        .ToListAsync();

        var model = new AdminClassListViewModel
        {
            Category = category,
            ShowInactive = showInactive,
            Classes = rows
        };

        return View(model);

    }

    public async Task<IActionResult> Create()
    {
        var model = new ClassFormViewModel
        {
            Capacity = 10,
            Instructors = await InstructorOptionsAsync()
        };

        return View("ClassForm", model);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassFormViewModel form) 
    {
        if (!ModelState.IsValid)
            return await ShowFormAsync(form);

        var startUtc = StudioTime.ToUtc(form.StartTime!.Value);

        if (startUtc <= DateTime.UtcNow)
        {
            ModelState.AddModelError(nameof(form.StartTime), "The start time must be in the future.");
            return await ShowFormAsync(form);
        }

        var fitnessClass = new FitnessClass(
            form.Title,
            form.Description,
            form.Category!.Value,
            form.InstructorId!.Value,
            startUtc,
            form.Capacity);

        db.FitnessClasses.Add(fitnessClass);
        await db.SaveChangesAsync();

        TempData["Message"] = $"{fitnessClass.Title} has been created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) 
    {
        var fitnessClass = await db.FitnessClasses.FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass is null)
            return NotFound();

        var model = new ClassFormViewModel
        {
            Id = fitnessClass.Id,
            Title = fitnessClass.Title,
            Description = fitnessClass.Description,
            Category = fitnessClass.Category,
            InstructorId = fitnessClass.InstructorId,
            StartTime = StudioTime.ToLocal(fitnessClass.StartTime),
            Capacity = fitnessClass.Capacity
        };

        return await ShowFormAsync(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClassFormViewModel form) 
    {
        if (!ModelState.IsValid)
            return await ShowFormAsync(form);

        var fitnessClass = await db.FitnessClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == form.Id);

        if (fitnessClass is null)
            return NotFound();

        var now = DateTime.UtcNow;
        var notifications = new List<Notification>();

        try
        {
            fitnessClass.UpdateDetails(
                form.Title,
                form.Description,
                form.Category!.Value,
                form.InstructorId!.Value,
                StudioTime.ToUtc(form.StartTime!.Value),
                now);

            if (form.Capacity != fitnessClass.Capacity)
                notifications = fitnessClass.ChangeCapacity(form.Capacity, now);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ShowFormAsync(form);
        }

        await db.SaveChangesAsync();

        foreach (var notification in notifications)
            await notifier.SendAsync(notification);

        TempData["Message"] = $"{fitnessClass.Title} has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelClass(int id) 
    {
        var fitnessClass = await db.FitnessClasses
        .Include(c => c.Bookings)
        .FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass is null)
            return NotFound();

        try
        {
            var notifications = fitnessClass.Cancel(DateTime.UtcNow);
            await db.SaveChangesAsync();

            foreach (var notification in notifications)
                await notifier.SendAsync(notification);

            TempData["Message"] = $"{fitnessClass.Title} has been cancelled and everyone booked was notified.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Roster(int id) 
    {
        var fitnessClass = await db.FitnessClasses
        .Include(c => c.Bookings)
        .FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass is null)
            return NotFound();

        var model = new RosterViewModel
        {
            ClassId = fitnessClass.Id,
            Title = fitnessClass.Title,
            StartTime = StudioTime.ToLocal(fitnessClass.StartTime),
            Capacity = fitnessClass.Capacity,
            IsCancelled = fitnessClass.IsCancelled,
            Confirmed = fitnessClass.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.CreatedAt)
                .Select(ToRosterEntry)
                .ToList(),
            Offered = fitnessClass.Bookings
                .Where(b => b.Status == BookingStatus.Offered)
                .OrderBy(b => b.HoldExpiresAt)
                .Select(ToRosterEntry)
                .ToList(),
            Waitlist = fitnessClass.Waitlist
                .Select(ToRosterEntry)
                .ToList()
        };

        return View(model);
    }

    public IActionResult CreateInstructor() 
    {
        return View(new InstructorFormViewModel());

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInstructor(InstructorFormViewModel form) 
    {
        if (!ModelState.IsValid)
            return View(form);

        var instructor = new Instructor
        {
            Name = form.Name.Trim(),
            Bio = string.IsNullOrWhiteSpace(form.Bio) ? null : form.Bio.Trim()
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync();

        TempData["Message"] = $"{instructor.Name} has been added.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ShowFormAsync(ClassFormViewModel form)
    {
        form.Instructors = await InstructorOptionsAsync();
        return View("ClassForm", form);
    }

    private Task<List<SelectListItem>> InstructorOptionsAsync() =>
    db.Instructors
        .OrderBy(i => i.Name)
        .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Name })
        .ToListAsync();

    private static RosterEntryViewModel ToRosterEntry(Booking booking) => new()
    {
        BookingId = booking.Id,
        Name = booking.MemberName,
        Email = booking.MemberEmail,
        CreatedAt = StudioTime.ToLocal(booking.CreatedAt),
        HoldExpiresAt = booking.HoldExpiresAt is null ? null : StudioTime.ToLocal(booking.HoldExpiresAt.Value)
    };
}
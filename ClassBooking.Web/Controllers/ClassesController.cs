using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassBooking.Web.Helpers;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Web.Controllers;

public class ClassesController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var model = await BuildDetailsAsync(id);

        if (model is null)
            return NotFound();

        return View(model); 

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book([Bind(Prefix = "BookingForm")] BookingFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var model = await BuildDetailsAsync(form.ClassId);

            if (model is null)
                return NotFound();

            model.BookingForm = form;
            return View("Details", model);
        }

        var fitnessClass = await db.FitnessClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == form.ClassId);

        if (fitnessClass is null)
            return NotFound();

        try
        {
            var booking = fitnessClass.Book(form.MemberName, form.MemberEmail, DateTime.UtcNow);
            await db.SaveChangesAsync();

            TempData["Message"] = booking.Status == BookingStatus.Confirmed
                ? "You're booked. See you there."
                : "The class is full, so you're on the waitlist. You'll get an email if a spot opens.";

            return RedirectToAction("Manage", "Bookings", new { id = booking.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = form.ClassId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FindBooking([Bind(Prefix = "FindBooking")] FindBookingViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var model = await BuildDetailsAsync(form.ClassId);

            if (model is null)
                return NotFound();

            model.FindBooking = form;
            return View("Details", model);
        }

        var email = form.Email.Trim().ToLowerInvariant();

        var booking = await db.Bookings
            .FirstOrDefaultAsync(b =>
                b.FitnessClassId == form.ClassId &&
                b.MemberEmail == email &&
                (b.Status == BookingStatus.Confirmed ||
                 b.Status == BookingStatus.Waitlisted ||
                 b.Status == BookingStatus.Offered));

        if (booking is null)
        {
            TempData["Error"] = "We couldn't find an active booking for that email in this class.";
            return RedirectToAction(nameof(Details), new { id = form.ClassId });
        }
        return RedirectToAction("Manage", "Bookings", new { id = booking.Id });
    }

    private async Task<ClassDetailsViewModel?> BuildDetailsAsync(int id)
    {
        var fitnessClass = await db.FitnessClasses
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass == null) return null;
       
        var now = DateTime.UtcNow;

        BookingState state;

        if (fitnessClass.IsCancelled)
            state = BookingState.Cancelled;
        else if (now >= fitnessClass.StartTime - FitnessClass.SignupCutoff)
            state = BookingState.Closed;
        else if (fitnessClass.AvailableSpots > 0 && !fitnessClass.Waitlist.Any())
            state = BookingState.Open;
        else if (now >= fitnessClass.StartTime - FitnessClass.MarketRelease)
            state = BookingState.Full;
        else if (fitnessClass.Waitlist.Count() >= FitnessClass.WaitlistLimit)
            state = BookingState.Full;
        else
            state = BookingState.WaitlistOnly;

        return new ClassDetailsViewModel
        {
            Id = fitnessClass.Id,
            Title = fitnessClass.Title,
            Description = fitnessClass.Description,
            Capacity = fitnessClass.Capacity,
            InstructorName = fitnessClass.Instructor.Name,
            SpotsLeft = fitnessClass.AvailableSpots,
            WaitlistCount = fitnessClass.Waitlist.Count(),
            State = state,
            StartTime = StudioTime.ToLocal(fitnessClass.StartTime),
            Category = fitnessClass.Category,

            BookingForm = new BookingFormViewModel { ClassId = id},
            FindBooking = new FindBookingViewModel { ClassId = id}
        };
    }
}
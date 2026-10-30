using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Helpers;
using ClassBooking.Web.Models;
using ClassBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Controllers;

public class BookingsController(AppDbContext db, INotificationService notifier) : Controller
{
    public async Task<IActionResult> Manage(int id)
    {
        var fitnessClass = await LoadClassForBookingAsync(id);

        if (fitnessClass is null)
            return NotFound();

        var booking = fitnessClass.Bookings.First(b => b.Id == id);
        var now = DateTime.UtcNow;

        var canCancel = booking.IsActive &&
            !(booking.Status == BookingStatus.Confirmed && now >= fitnessClass.StartTime - FitnessClass.MarketRelease);

        var canClaim = booking.Status == BookingStatus.Offered && now < booking.HoldExpiresAt;

        var model = new ManageBookingViewModel
        {
            BookingId = booking.Id,
            ClassTitle = fitnessClass.Title,
            StartTime = StudioTime.ToLocal(fitnessClass.StartTime),
            Status = booking.Status,
            HoldExpiresAt = booking.HoldExpiresAt is null ? null : StudioTime.ToLocal(booking.HoldExpiresAt.Value),
            CanCancel = canCancel,
            CanClaim = canClaim
        };

        return View(model);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var fitnessClass = await LoadClassForBookingAsync(id);

        if (fitnessClass is null)
            return NotFound();

        var booking = fitnessClass.Bookings.First(b => b.Id == id);

        try
        {
            var notifications = fitnessClass.CancelBooking(booking, DateTime.UtcNow);
            await db.SaveChangesAsync();


            foreach (var notification in notifications)
                await db.SaveChangesAsync();

            TempData["Message"] = "You're no longer booked.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(int id)
    {
        var fitnessClass = await LoadClassForBookingAsync(id);

        if (fitnessClass is null)
            return NotFound();

        var booking = fitnessClass.Bookings.First(b => b.Id == id);

        try
        {
            fitnessClass.ClaimOffer(booking, DateTime.UtcNow);
            await db.SaveChangesAsync();
            TempData["Message"] = "You've claimed an offered spot.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id });
    }

    private Task<FitnessClass?> LoadClassForBookingAsync(int bookingId) =>
        db.FitnessClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Bookings.Any(b => b.Id == bookingId));
}
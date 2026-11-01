using ClassBooking.Core;
using ClassBooking.Web.Data;
using ClassBooking.Web.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Controllers.Api;

[ApiController]
[Route("api/classes")]
public class ClassesApiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClassDto>>> GetClasses(Category? category, DateTime? from, bool onlyOpen)
    {
        var now = DateTime.UtcNow;

        var query = db.FitnessClasses
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .Where(c => !c.IsCancelled && c.StartTime > now);

        if (category is not null)
            query = query.Where(c => c.Category == category);

        if (from is not null)
            query = query.Where(c => c.StartTime >= from.Value);

        var classes = await query
            .OrderBy(c => c.StartTime)
            .ToListAsync();

        if (onlyOpen)
            classes = classes.Where(c => c.AvailableSpots > 0).ToList();

        return classes.Select(ToClassDto).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClassDto>> GetClass(int id)
    {
        var fitnessClass = await db.FitnessClasses
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass is null)
            return NotFound();

        return ToClassDto(fitnessClass);
    }

    [HttpPost("{id}/bookings")]
    public async Task<ActionResult<BookingDto>> Book(int id, CreateBookingRequest request)
    {
        var fitnessClass = await db.FitnessClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (fitnessClass is null)
            return NotFound();

        try
        {
            var booking = fitnessClass.Book(request.MemberName, request.MemberEmail, DateTime.UtcNow);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClass), new { id }, ToBookingDto(booking));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Booking refused",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    private static ClassDto ToClassDto(FitnessClass fitnessClass) => new()
    {
        Id = fitnessClass.Id,
        Title = fitnessClass.Title,
        Description = fitnessClass.Description,
        Category = fitnessClass.Category.ToString(),
        InstructorName = fitnessClass.Instructor.Name,
        StartTime = AsUtc(fitnessClass.StartTime),
        Capacity = fitnessClass.Capacity,
        SpotsLeft = fitnessClass.AvailableSpots,
        WaitlistCount = fitnessClass.Waitlist.Count(),
        IsCancelled = fitnessClass.IsCancelled
    };

    private static BookingDto ToBookingDto(Booking booking) => new()
    {
        Id = booking.Id,
        ClassId = booking.FitnessClassId,
        MemberName = booking.MemberName,
        MemberEmail = booking.MemberEmail,
        Status = booking.Status.ToString(),
        CreatedAt = AsUtc(booking.CreatedAt),
        HoldExpiresAt = booking.HoldExpiresAt is null ? null : AsUtc(booking.HoldExpiresAt.Value)
    };

    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}

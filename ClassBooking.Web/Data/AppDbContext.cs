using ClassBooking.Core;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Instructor> Instructors => Set<Instructor>();
}

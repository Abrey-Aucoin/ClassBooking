using ClassBooking.Core;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<FitnessClass> FitnessClasses => Set<FitnessClass>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        modelBuilder.Entity<FitnessClass>()
            .Property(c => c.Category)
            .HasConversion<string>()
            .HasMaxLength(20);
        modelBuilder.Entity<FitnessClass>().Ignore(c => c.Waitlist);
    }

}

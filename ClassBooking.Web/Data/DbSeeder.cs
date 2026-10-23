using ClassBooking.Core;
using Microsoft.EntityFrameworkCore;

namespace ClassBooking.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Instructors.AnyAsync())
            return;

        var zara = new Instructor { Name = "Zara Patel", Bio = "Vinyasa and restorative yoga, ten years teaching." };
        var emma = new Instructor { Name = "Emma Brooks", Bio = "Former dancer who teaches barre and Pilates." };
        var marcus = new Instructor { Name = "Marcus Lee", Bio = "Boxing coach and spin instructor." };

        db.Instructors.AddRange(zara, emma, marcus);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var tomorrow = now.Date.AddDays(1);

        var morningFlow = new FitnessClass("Morning Flow", "An easy-paced vinyasa class to start the day.", Category.Yoga, zara.Id, tomorrow.AddHours(13), 12);
        var restorative = new FitnessClass("Restorative Yoga", "Slow, supported poses to wind down.", Category.Yoga, zara.Id, tomorrow.AddDays(1).AddHours(23), 10);
        var barre = new FitnessClass("Barre Basics", "Low-impact strength work at the barre.", Category.Barre, emma.Id, tomorrow.AddHours(15), 8);
        var pilates = new FitnessClass("Mat Pilates", "Core-focused mat work for all levels.", Category.Pilates, emma.Id, tomorrow.AddDays(2).AddHours(14), 10);
        var spin = new FitnessClass("Lunch Spin", "Forty-five minutes of intervals.", Category.Spin, marcus.Id, tomorrow.AddHours(17), 2);
        var boxing = new FitnessClass("Boxing Fundamentals", "Footwork, stance and combinations.", Category.Boxing, marcus.Id, tomorrow.AddDays(3).AddHours(23), 1);

        db.FitnessClasses.AddRange(morningFlow, restorative, barre, pilates, spin, boxing);

        spin.Book("Jordan Hale", "jordan@example.com", now.AddHours(-2));
        spin.Book("Priya Shah", "priya@example.com", now.AddHours(-2).AddMinutes(1));
        spin.Book("Leo Grant", "leo@example.com", now.AddHours(-2).AddMinutes(2));
        spin.Book("Mia Torres", "mia@example.com", now.AddHours(-2).AddMinutes(3));

        var tom = boxing.Book("Tom Reyes", "tom@example.com", now.AddHours(-8));
        boxing.Book("Sam Cole", "sam@example.com", now.AddHours(-8).AddMinutes(1));
        boxing.Book("Eric Dunn", "eric@example.com", now.AddHours(-8).AddMinutes(2));
        boxing.CancelBooking(tom, now.AddHours(-7));

        await db.SaveChangesAsync();
    }
}

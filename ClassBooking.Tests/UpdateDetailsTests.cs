using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class UpdateDetailsTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void UpdateDetails_ChangesFields()
        {
            var fitnessClass = CreateClass();

            fitnessClass.UpdateDetails("Evening Flow", "Slower pace", Category.Pilates, 2, Start, DayBefore);

            Assert.Equal("Evening Flow", fitnessClass.Title);
            Assert.Equal("Slower pace", fitnessClass.Description);
            Assert.Equal(Category.Pilates, fitnessClass.Category);
            Assert.Equal(2, fitnessClass.InstructorId);
        }

        [Fact]
        public void UpdateDetails_NewStartTimeWithNoBookings_Succeeds()
        {
            var fitnessClass = CreateClass();
            var newStart = Start.AddDays(1);

            fitnessClass.UpdateDetails("Morning Flow", null, Category.Yoga, 1, newStart, DayBefore);

            Assert.Equal(newStart, fitnessClass.StartTime);
        }

        [Fact]
        public void UpdateDetails_NewStartTimeWithActiveBooking_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.UpdateDetails("Morning Flow", null, Category.Yoga, 1, Start.AddDays(1), DayBefore));

            Assert.Contains("once people have booked", ex.Message);
        }

        [Fact]
        public void UpdateDetails_SameStartTimeWithBookings_Succeeds()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            fitnessClass.UpdateDetails("Morning Flow Fixed", null, Category.Yoga, 1, Start, DayBefore.AddHours(1));

            Assert.Equal("Morning Flow Fixed", fitnessClass.Title);
        }

        [Fact]
        public void UpdateDetails_NewStartTimeWithOnlyCancelledBookings_Succeeds()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));
            var newStart = Start.AddDays(1);

            fitnessClass.UpdateDetails("Morning Flow", null, Category.Yoga, 1, newStart, DayBefore.AddHours(2));

            Assert.Equal(newStart, fitnessClass.StartTime);
        }

        [Fact]
        public void UpdateDetails_CancelledClass_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Cancel(DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.UpdateDetails("Evening Flow", null, Category.Yoga, 1, Start, DayBefore.AddHours(1)));

            Assert.Contains("cancelled", ex.Message);
        }

        [Fact]
        public void UpdateDetails_NewStartTimeInPast_Throws()
        {
            var fitnessClass = CreateClass();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.UpdateDetails("Morning Flow", null, Category.Yoga, 1, DayBefore.AddHours(-1), DayBefore));

            Assert.Contains("must be in the future", ex.Message);
        }

        [Fact]
        public void UpdateDetails_AfterClassStarted_Throws()
        {
            var fitnessClass = CreateClass();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.UpdateDetails("Evening Flow", null, Category.Yoga, 1, Start, Start.AddMinutes(10)));

            Assert.Contains("started", ex.Message);
        }
    }
}
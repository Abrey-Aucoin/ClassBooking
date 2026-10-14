using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class BookTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);

        private static FitnessClass CreateClass(int capacity = 2) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void Book_WithOpenSpot_IsConfirmed()
        {
            var fitnessClass = CreateClass();

            var booking = fitnessClass.Book("Sam", "sam@test.com", Start.AddDays(-1));

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
        }

        [Fact]
        public void Book_SameEmailTwice_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Sam", "sam@test.com", Start.AddDays(-1));

            Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Book("Sam", " SAM@test.com", Start.AddDays(-1)));
        }

        [Fact]
        public void Book_WhenFull_IsWaitlisted()
        {
            var fitnessClass = CreateClass(capacity: 1);
            fitnessClass.Book("Sam", "sam@test.com", Start.AddDays(-1));
            var booking = fitnessClass.Book("eric", "eric@test.com", Start.AddDays(-1));


            Assert.Equal(BookingStatus.Waitlisted, booking.Status);
        }

        [Fact]
        public void Book_WaitlistFull_Throws()
        {
            var fitnessClass = CreateClass(capacity: 1);
            fitnessClass.Book("Tom", "tom@test.com", Start.AddDays(-1));
            for (var i = 0; i < FitnessClass.WaitlistLimit; i++)
                fitnessClass.Book($"Member {i}", $"member{i}@test.com", Start.AddDays(-1));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Book("Sam", "sam@test.com", Start.AddDays(-1)));

            Assert.Contains("waitlist", ex.Message);
        }

        [Fact]
        public void Book_WhenFullInLastHour_Throws()
        {
            var fitnessClass = CreateClass(capacity: 1);
            fitnessClass.Book("Tom", "tom@test.com", Start.AddDays(-1));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Book("Sam", "sam@test.com", Start.AddMinutes(-30)));

            Assert.Equal("This class is full.", ex.Message);
        }

        [Fact]
        public void Book_AtSignupCutoff_Throws()
        {
            var fitnessClass = CreateClass();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Book("Sam", "sam@test.com", Start.AddMinutes(-5)));

            Assert.Contains("closed", ex.Message);
        }

        [Fact]
        public void Book_JustBeforeSignupCutoff_IsConfirmed()
        {
            var fitnessClass = CreateClass();

            var booking = fitnessClass.Book("Sam", "sam@test.com", Start.AddMinutes(-6));

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
        }

    }
}

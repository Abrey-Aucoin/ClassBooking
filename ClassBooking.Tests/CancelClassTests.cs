using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class CancelClassTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void Cancel_WithActiveBookings_CancelsAndNotifiesAll()
        {
            var fitnessClass = CreateClass(capacity: 2);
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            var ava = fitnessClass.Book("Ava", "ava@test.com", DayBefore.AddMinutes(3));
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            var notifications = fitnessClass.Cancel(DayBefore.AddHours(2));

            Assert.Equal(BookingStatus.Cancelled, sam.Status);
            Assert.Equal(BookingStatus.Cancelled, eric.Status);
            Assert.Equal(BookingStatus.Cancelled, ava.Status);
            Assert.Equal(3, notifications.Count);
            Assert.True(fitnessClass.IsCancelled);
        }

        [Fact]
        public void Cancel_AlreadyCancelledBooking_IsNotNotified()
        {
            var fitnessClass = CreateClass(capacity: 2);
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            var notifications = fitnessClass.Cancel(DayBefore.AddHours(2));

            Assert.Equal("sam@test.com", Assert.Single(notifications).Email);
            Assert.Equal(BookingStatus.Cancelled, tom.Status);
        }

        [Fact]
        public void Cancel_WithNoBookings_MarksClassCancelled()
        {
            var fitnessClass = CreateClass();

            var notifications = fitnessClass.Cancel(DayBefore);

            Assert.Empty(notifications);
            Assert.True(fitnessClass.IsCancelled);
        }

        [Fact]
        public void Cancel_Twice_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Cancel(DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Cancel(DayBefore.AddHours(1)));

            Assert.Contains("already been cancelled", ex.Message);
        }

        [Fact]
        public void Cancel_AtStartTime_Throws()
        {
            var fitnessClass = CreateClass();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Cancel(Start));

            Assert.Contains("after it has started", ex.Message);
        }

        [Fact]
        public void Book_AfterClassCancelled_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Cancel(DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddHours(1)));

            Assert.Contains("cancelled", ex.Message);
        }

        [Fact]
        public void ProcessExpiredHolds_AfterClassCancelled_DoesNothing()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Cancel(DayBefore.AddHours(1));

            var notifications = fitnessClass.ProcessExpiredHolds(Start.AddHours(-1));

            Assert.Empty(notifications);
        }
    }
}
using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class ChangeCapacityTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void ChangeCapacity_RaisedWithWaitlist_OffersNewSpotsInOrder()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            var ava = fitnessClass.Book("Ava", "ava@test.com", DayBefore.AddMinutes(3));

            var notifications = fitnessClass.ChangeCapacity(3, DayBefore.AddHours(1));

            Assert.Equal(BookingStatus.Offered, sam.Status);
            Assert.Equal(BookingStatus.Offered, eric.Status);
            Assert.Equal(BookingStatus.Waitlisted, ava.Status);
            Assert.Equal(2, notifications.Count);
        }

        [Fact]
        public void ChangeCapacity_RaisedWithEmptyWaitlist_OpensSpots()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            var notifications = fitnessClass.ChangeCapacity(3, DayBefore.AddHours(1));

            Assert.Empty(notifications);
            Assert.Equal(2, fitnessClass.AvailableSpots);
        }

        [Fact]
        public void ChangeCapacity_BelowConfirmed_Throws()
        {
            var fitnessClass = CreateClass(capacity: 3);
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.ChangeCapacity(2, DayBefore.AddHours(1)));

            Assert.Contains("can't be lower", ex.Message);
        }

        [Fact]
        public void ChangeCapacity_BelowConfirmedPlusOffered_Throws()
        {
            var fitnessClass = CreateClass(capacity: 2);
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.ChangeCapacity(1, DayBefore.AddHours(2)));

            Assert.Contains("can't be lower", ex.Message);
        }

        [Fact]
        public void ChangeCapacity_LoweredToConfirmedCount_Succeeds()
        {
            var fitnessClass = CreateClass(capacity: 3);
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));

            fitnessClass.ChangeCapacity(2, DayBefore.AddHours(1));

            Assert.Equal(2, fitnessClass.Capacity);
            Assert.Equal(0, fitnessClass.AvailableSpots);
        }

        [Fact]
        public void ChangeCapacity_RaisedInLastHour_OffersNothing()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));

            var notifications = fitnessClass.ChangeCapacity(2, Start.AddMinutes(-30));

            Assert.Empty(notifications);
            Assert.Equal(BookingStatus.Waitlisted, sam.Status);
        }
    }
}
using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class ProcessExpiredHoldsTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void ProcessExpiredHolds_HoldRanOut_OffersNextInLine()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            var offeredAt = DayBefore.AddHours(1);
            fitnessClass.CancelBooking(tom, offeredAt);

            var notifications = fitnessClass.ProcessExpiredHolds(offeredAt + FitnessClass.HoldDuration);

            Assert.Equal(BookingStatus.Expired, sam.Status);
            Assert.Equal(BookingStatus.Offered, eric.Status);
            Assert.Equal("eric@test.com", Assert.Single(notifications).Email);
        }

        [Fact]
        public void ProcessExpiredHolds_HoldStillActive_ChangesNothing()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            var offeredAt = DayBefore.AddHours(1);
            fitnessClass.CancelBooking(tom, offeredAt);

            var notifications = fitnessClass.ProcessExpiredHolds(offeredAt + FitnessClass.HoldDuration - TimeSpan.FromMinutes(1));

            Assert.Empty(notifications);
            Assert.Equal(BookingStatus.Offered, sam.Status);
            Assert.Equal(BookingStatus.Waitlisted, eric.Status);
        }

        [Fact]
        public void ProcessExpiredHolds_HoldRanOutWithNoOneWaiting_OpensSpot()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var offeredAt = DayBefore.AddHours(1);
            fitnessClass.CancelBooking(tom, offeredAt);

            var notifications = fitnessClass.ProcessExpiredHolds(offeredAt + FitnessClass.HoldDuration);

            Assert.Empty(notifications);
            Assert.Equal(BookingStatus.Expired, sam.Status);
            Assert.Equal(1, fitnessClass.AvailableSpots);
        }

        [Fact]
        public void ProcessExpiredHolds_AtOneHourMark_ClearsWaitlistAndNotifiesEveryone()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            fitnessClass.CancelBooking(tom, Start.AddHours(-3));

            var notifications = fitnessClass.ProcessExpiredHolds(Start.AddHours(-1));

            Assert.Equal(BookingStatus.Expired, sam.Status);
            Assert.Equal(BookingStatus.Expired, eric.Status);
            Assert.Equal(2, notifications.Count);
            Assert.Contains(notifications, n => n.Email == "sam@test.com");
            Assert.Contains(notifications, n => n.Email == "eric@test.com");
            Assert.Equal(1, fitnessClass.AvailableSpots);
        }

        [Fact]
        public void ProcessExpiredHolds_RunTwiceInLastHour_SecondRunDoesNothing()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            fitnessClass.CancelBooking(tom, Start.AddHours(-3));
            fitnessClass.ProcessExpiredHolds(Start.AddHours(-1));

            var notifications = fitnessClass.ProcessExpiredHolds(Start.AddMinutes(-55));

            Assert.Empty(notifications);
        }

        [Fact]
        public void ProcessExpiredHolds_AfterRelease_NewMemberCanBook()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.CancelBooking(tom, Start.AddHours(-3));
            fitnessClass.ProcessExpiredHolds(Start.AddHours(-1));

            var ava = fitnessClass.Book("Ava", "ava@test.com", Start.AddMinutes(-30));

            Assert.Equal(BookingStatus.Confirmed, ava.Status);
        }
    }
}

using ClassBooking.Core;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace ClassBooking.Tests
{
    public class CancelBookingTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void CancelBooking_ConfirmedWithWaitlist_OffersFirstInLine()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "Tom@test.com",DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com",DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com",DayBefore.AddMinutes(2));

            var notifications = fitnessClass.CancelBooking(tom,DayBefore.AddHours(1));

            Assert.Equal(BookingStatus.Offered, sam.Status);
            Assert.Equal(BookingStatus.Waitlisted, eric.Status);

            var notification = Assert.Single(notifications);
            Assert.Equal("sam@test.com", notification.Email);

        }

        [Fact]
        public void CancelBooking_ConfirmedWithEmptyWaitlist_OpensSpot()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            var notifications = fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            Assert.Empty(notifications);
            Assert.Equal(1, fitnessClass.AvailableSpots);
        }

        [Fact]
        public void CancelBooking_ConfirmedAtOneHourCutoff_Throws()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.CancelBooking(tom, Start.AddHours(-1)));

            Assert.Contains("within an hour", ex.Message);
        }

        [Fact]
        public void CancelBooking_ConfirmedJustBeforeCutoff_Succeeds()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);

            fitnessClass.CancelBooking(tom, Start.AddMinutes(-61));

            Assert.Equal(BookingStatus.Cancelled, tom.Status);
        }

        [Fact]
        public void CancelBooking_Waitlisted_OffersNothing()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));

            var notifications = fitnessClass.CancelBooking(sam, DayBefore.AddHours(1));

            Assert.Empty(notifications);
            Assert.Equal(BookingStatus.Cancelled, sam.Status);
            Assert.Equal(BookingStatus.Waitlisted, eric.Status);
        }

        [Fact]
        public void CancelBooking_Offered_OffersNextInLine()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var eric = fitnessClass.Book("Eric", "eric@test.com", DayBefore.AddMinutes(2));
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            var notifications = fitnessClass.CancelBooking(sam, DayBefore.AddHours(2));

            Assert.Equal(BookingStatus.Offered, eric.Status);
            Assert.Equal("eric@test.com", Assert.Single(notifications).Email);
        }

        [Fact]
        public void CancelBooking_CloseToStart_HoldEndsAtOneHourMark()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));

            fitnessClass.CancelBooking(tom, Start.AddHours(-3));

            Assert.Equal(Start.AddHours(-1), sam.HoldExpiresAt);
        }

        [Fact]
        public void CancelBooking_FarFromStart_HoldIsFullDuration()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", Start.AddDays(-3));
            var sam = fitnessClass.Book("Sam", "sam@test.com", Start.AddDays(-3).AddMinutes(1));
            var now = Start.AddDays(-2);

            fitnessClass.CancelBooking(tom, now);

            Assert.Equal(now + FitnessClass.HoldDuration, sam.HoldExpiresAt);
        }

        [Fact]
        public void CancelBooking_BookingFromAnotherClass_Throws()
        {
            var classA = CreateClass();
            var classB = CreateClass();
            var booking = classB.Book("Tom", "tom@test.com", DayBefore);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                classA.CancelBooking(booking, DayBefore.AddHours(1)));

            Assert.Contains("does not belong", ex.Message);
        }

        [Fact]
        public void CancelBooking_ThenSameEmailBooksAgain_IsConfirmed()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            var rebooked = fitnessClass.Book("Tom", "tom@test.com", DayBefore.AddHours(2));

            Assert.Equal(BookingStatus.Confirmed, rebooked.Status);
        }


    }
}
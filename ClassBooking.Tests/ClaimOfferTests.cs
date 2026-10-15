using ClassBooking.Core;

namespace ClassBooking.Tests
{
    public class ClaimOfferTests
    {
        private static readonly DateTime Start = new(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DayBefore = Start.AddDays(-1);

        private static FitnessClass CreateClass(int capacity = 1) =>
            new("Morning Flow", null, Category.Yoga, 1, Start, capacity);

        [Fact]
        public void ClaimOffer_WithinHold_IsConfirmed()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            fitnessClass.CancelBooking(tom, DayBefore.AddHours(1));

            fitnessClass.ClaimOffer(sam, DayBefore.AddHours(2));

            Assert.Equal(BookingStatus.Confirmed, sam.Status);
            Assert.Null(sam.HoldExpiresAt);
        }

        [Fact]
        public void ClaimOffer_AtHoldExpiry_Throws()
        {
            var fitnessClass = CreateClass();
            var tom = fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));
            var offeredAt = DayBefore.AddHours(1);
            fitnessClass.CancelBooking(tom, offeredAt);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.ClaimOffer(sam, offeredAt + FitnessClass.HoldDuration));

            Assert.Contains("expired", ex.Message);
        }

        [Fact]
        public void ClaimOffer_WaitlistedWithoutOffer_Throws()
        {
            var fitnessClass = CreateClass();
            fitnessClass.Book("Tom", "tom@test.com", DayBefore);
            var sam = fitnessClass.Book("Sam", "sam@test.com", DayBefore.AddMinutes(1));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                fitnessClass.ClaimOffer(sam, DayBefore.AddHours(1)));

            Assert.Contains("open offer", ex.Message);
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ClassBooking.Core
{
    public class Booking
    {
        public int Id { get; private set; }

        public int FitnessClassId { get; private set; }

        [MaxLength(100)]
        public string MemberName { get; private set; } = string.Empty;

        [MaxLength(254)]
        public string MemberEmail { get; private set; } = string.Empty;

        public BookingStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? HoldExpiresAt { get; private set; }

        public bool IsActive => Status is BookingStatus.Confirmed or BookingStatus.Waitlisted or BookingStatus.Offered;

        internal Booking(string memberName, string memberEmail, BookingStatus status, DateTime createdAt)
        {
            MemberName = memberName;
            MemberEmail = memberEmail;
            Status = status;
            CreatedAt = createdAt;
        }

        private Booking() { }

        internal void Offer(DateTime expiresAt)
        {
            if (Status != BookingStatus.Waitlisted)
                throw new InvalidOperationException("Only waitlisted bookings can be offered a spot.");

            Status = BookingStatus.Offered;
            HoldExpiresAt = expiresAt;
        }

        internal void ClaimOffer()
        {
            if (Status != BookingStatus.Offered)
                throw new InvalidOperationException("Only offered bookings can be confirmed.");

            Status = BookingStatus.Confirmed;
            HoldExpiresAt = null;
        }

        internal void Expire()
        {
            if (Status is not (BookingStatus.Waitlisted or BookingStatus.Offered))
                throw new InvalidOperationException("Only waitlisted or offered bookings can expire.");

            Status = BookingStatus.Expired;
            HoldExpiresAt = null;
        }

        internal void Cancel()
        {
            if (!IsActive)
                throw new InvalidOperationException("This booking is already inactive.");

            Status = BookingStatus.Cancelled;
            HoldExpiresAt = null;
        }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;

namespace ClassBooking.Core
{
    public class FitnessClass
    {
        public const int WaitlistLimit = 5;
        public static readonly TimeSpan HoldDuration = TimeSpan.FromHours(6);
        public static readonly TimeSpan SignupCutoff = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MarketRelease = TimeSpan.FromHours(1);

        public readonly List<Booking> _bookings = [];

        public int Id { get; private set; }

        [MaxLength(100)]
        public string Title { get; private set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; private set; }

        public Category Category { get; private set; }

        public int InstructorId { get; private set; }

        public Instructor Instructor { get; private set; } = null!;

        public DateTime StartTime { get; private set; }

        public int Capacity { get; private set; }

        public bool IsCancelled { get; private set; }

        public IReadOnlyCollection<Booking> Bookings => _bookings;

        public int ConfirmedCount => _bookings.Count(b => b.Status == BookingStatus.Confirmed);

        public int OfferedCount => _bookings.Count(b => b.Status == BookingStatus.Offered);

        public IEnumerable<Booking> Waitlist => _bookings
            .Where(b => b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.CreatedAt);

        public int AvailableSpots => Capacity - ConfirmedCount - OfferedCount;

        public FitnessClass(string title, string? description, Category category, int instructorId, DateTime startTime, int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");

            Title = title;
            Description = description;
            Category = category;
            InstructorId = instructorId;
            StartTime = startTime;
            Capacity = capacity;
        }

        private FitnessClass() { }

        public Booking Book(string memberName, string memberEmail, DateTime now)
        {
            if (IsCancelled)
                throw new InvalidOperationException("This class has been cancelled.");

            if (now >= StartTime - SignupCutoff)
                throw new InvalidOperationException("Sign-up for this class is closed.");

            var email = memberEmail.Trim().ToLowerInvariant();

            if (_bookings.Any(b => b.IsActive && b.MemberEmail == email))
                throw new InvalidOperationException("This email already has a booking for this class.");

            BookingStatus status;

            if (AvailableSpots > 0 && !Waitlist.Any())
                status = BookingStatus.Confirmed;
            else if (now >= StartTime - MarketRelease)
                throw new InvalidOperationException("This class is full.");
            else if (Waitlist.Count() >= WaitlistLimit)
                throw new InvalidOperationException("The waitlist for this class is full.");
            else
                status = BookingStatus.Waitlisted;

            var booking = new Booking(memberName.Trim(), email, status, now);
            _bookings.Add(booking);
            return booking;
        }
        

        // cancel bookings
        public List<Notification> CancelBooking(Booking booking, DateTime now)
        {
            var notifications = new List<Notification>();
            if (!_bookings.Contains(booking))
                throw new InvalidOperationException("This booking does not belong to this class.");
            if (booking.Status == BookingStatus.Confirmed && now >= StartTime - MarketRelease)
                throw new InvalidOperationException("Bookings can't be cancelled within an hour of the class.");
            var heldSpot = booking.Status is BookingStatus.Confirmed or BookingStatus.Offered;
            booking.Cancel();

            if (heldSpot && now < StartTime - MarketRelease)
                OfferOpenSpots(now, notifications);

            return notifications;
        }

        // Offer newly opened spots to waitlist
        private void OfferOpenSpots(DateTime now, List<Notification> notifications)
        {
            var fullHold = now + HoldDuration;
            var releaseTime = StartTime - MarketRelease;
            var holdEnd = fullHold < releaseTime ? fullHold : releaseTime;
            while (AvailableSpots > 0 && Waitlist.Any())
            {
                var next = Waitlist.First();
                next.Offer(holdEnd);
                notifications.Add(new Notification(next.MemberEmail, $"A spot opened up for {Title}", $"Claim before {holdEnd:g}."));
            }

        }
    }
}

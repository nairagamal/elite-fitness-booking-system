namespace elite.Models
{
    public class OnlineSessionSchedule
    {
        public int Id { get; set; }
        public int OnlineSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableSlots { get; set; }

        public virtual OnlineSession OnlineSession { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}

namespace elite.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } // "Class" or "OnlineSession"
        public int ScheduleId { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } // Confirmed/Cancelled/Completed
        public decimal HoursConsumed { get; set; }

        public virtual User User { get; set; }
    }
}

namespace elite.Models
{
    public class ClassSchedule
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public int AvailableSlots { get; set; }

        public virtual Class Class { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}

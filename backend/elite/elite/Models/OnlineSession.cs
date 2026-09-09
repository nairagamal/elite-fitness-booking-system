namespace elite.Models
{
    public class OnlineSession
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Duration { get; set; } // Now in minutes
        public int TrainerId { get; set; }
        public decimal Price { get; set; }

        public virtual Trainer Trainer { get; set; }
        public virtual ICollection<OnlineSessionSchedule> Schedules { get; set; }
    }
}

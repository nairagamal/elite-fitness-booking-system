namespace elite.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string Certifications { get; set; }
        public string Bio { get; set; }
        public string ImageUrl { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<OnlineSession> OnlineSessions { get; set; }
    }
}

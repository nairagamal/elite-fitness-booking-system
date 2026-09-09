using System.ComponentModel.DataAnnotations;

namespace elite.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalHours { get; set; }       // Change this to decimal too!
        public decimal RemainingHours { get; set; }   // Changed to decimal
        public string Status { get; set; }
        public decimal PricePaid { get; set; }

        public virtual User User { get; set; }
    }
}

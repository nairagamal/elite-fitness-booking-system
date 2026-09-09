using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class MembershipDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalHours { get; set; }      // Change to decimal
        public decimal RemainingHours { get; set; }  // Change to decimal
        public string Status { get; set; }
        public decimal PricePaid { get; set; }
    }

    public class PurchaseMembershipDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Type { get; set; }
    }

}

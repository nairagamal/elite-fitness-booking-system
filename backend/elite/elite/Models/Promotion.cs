// Models/Promotion.cs
namespace elite.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; } // Percentage/Fixed
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }

        // Add this new property
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; }
    }

    // Add this new model to track promotion usage by users
    public class PromotionUsage
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int UserId { get; set; }
        public DateTime UsedAt { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal DiscountApplied { get; set; }

        public virtual Promotion Promotion { get; set; }
        public virtual User User { get; set; }
    }
}
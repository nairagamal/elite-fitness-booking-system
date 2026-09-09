using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
    }

    public class PromotionCreateDto
    {
        [Required, MaxLength(20)]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string DiscountType { get; set; }

        [Required, Range(0, 100)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 1000)]
        public int UsageLimit { get; set; } = 100;
    }

    // DTOs/ApplyPromotionDto.cs
    public class ApplyPromotionDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public decimal OrderAmount { get; set; }

    }
}

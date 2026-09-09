using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class MembershipTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMonths { get; set; }
        public decimal TotalHours { get; set; }  // Change from int to decimal
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class MembershipTypeCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(1, 12)]
        public int DurationMonths { get; set; }

        [Required, Range(1, 100)]
        public decimal TotalHours { get; set; }  // Change from int to decimal

        [Required, Range(0, 1000)]
        public decimal Price { get; set; }
    }

}

// DTOs/ProductUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class ProductUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(0, 1000)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; }

        [Range(0, 1000)]
        public int StockQuantity { get; set; }

        public IFormFile? ImageFile { get; set; } // Optional for updates
        public string? ExistingImageUrl { get; set; } // To track existing image
    }
}
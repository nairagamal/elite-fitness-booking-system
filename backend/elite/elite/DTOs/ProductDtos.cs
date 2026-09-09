using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int StockQuantity { get; set; }
    }

    // From your DTOs/ProductCreateDto.cs
    public class ProductCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required, Range(0, 1000)]
        public decimal Price { get; set; }
        public IFormFile ImageFile { get; set; } // For file upload
        [Required]
        public string Category { get; set; }
        [Range(0, 1000)]
        public int StockQuantity { get; set; }
    }
}

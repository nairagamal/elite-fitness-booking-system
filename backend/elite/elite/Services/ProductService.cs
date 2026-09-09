using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    // Services/ProductService.cs
    public class ProductService : IProductService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IImageService _imageService;

        public ProductService(GymDbContext context, ILogger<ProductService> logger, IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _imageService = imageService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category == category)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new ArgumentException("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto)
        {
            string imageUrl = null;

            // Handle image upload if provided
            if (productCreateDto.ImageFile != null)
            {
                imageUrl = await _imageService.UploadImageAsync(productCreateDto.ImageFile, "products");
            }

            var product = new Product
            {
                Name = productCreateDto.Name,
                Description = productCreateDto.Description,
                Price = productCreateDto.Price,
                ImageUrl = imageUrl, // Use the uploaded image URL
                Category = productCreateDto.Category,
                StockQuantity = productCreateDto.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto productUpdateDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new ArgumentException("Product not found");

            // Handle image update ONLY if a new image is provided
            if (productUpdateDto.ImageFile != null && productUpdateDto.ImageFile.Length > 0)
            {
                try
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        _imageService.DeleteImage(product.ImageUrl);
                    }

                    // Upload new image
                    product.ImageUrl = await _imageService.UploadImageAsync(productUpdateDto.ImageFile, "products");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating image for product");
                    // Keep existing image if upload fails
                }
            }
            // If no new image is provided, keep the existing image URL

            // Update other properties
            product.Name = productUpdateDto.Name;
            product.Description = productUpdateDto.Description;
            product.Price = productUpdateDto.Price;
            product.Category = productUpdateDto.Category;
            product.StockQuantity = productUpdateDto.StockQuantity;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new ArgumentException("Product not found");

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }

        // Services/ProductService.cs
        public async Task<bool> UpdateProductStockAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) throw new ArgumentException("Product not found");

            product.StockQuantity = quantity;
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

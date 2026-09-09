using elite.DTOs;

namespace elite.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto);
        Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto productUpdateDto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductStockAsync(int id, int quantity);
    }
}

using elite.DTOs;

namespace elite.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();
        Task<PromotionDto> GetPromotionByIdAsync(int id);
        Task<PromotionDto> GetPromotionByCodeAsync(string code);
        Task<PromotionDto> CreatePromotionAsync(PromotionCreateDto promotionCreateDto);
        Task<PromotionDto> UpdatePromotionAsync(int id, PromotionCreateDto promotionUpdateDto);
        Task<bool> DeletePromotionAsync(int id);
        Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, int userId); // Updated
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, int userId); // Updated
        Task<bool> HasUserUsedPromotionAsync(int promotionId, int userId); // New method
    }
}

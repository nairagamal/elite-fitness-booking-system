using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    // Services/PromotionService.cs
    public class PromotionService : IPromotionService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(GymDbContext context, ILogger<PromotionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
        {
            return await _context.Promotions
                .Select(p => new PromotionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    UsageLimit = p.UsageLimit,
                    TimesUsed = p.TimesUsed
                })
                .ToListAsync();
        }

        public async Task<PromotionDto> GetPromotionByIdAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) throw new ArgumentException("Promotion not found");

            return new PromotionDto
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                TimesUsed = promotion.TimesUsed
            };
        }

        public async Task<PromotionDto> GetPromotionByCodeAsync(string code)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code);

            if (promotion == null) throw new ArgumentException("Promotion not found");

            return new PromotionDto
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                TimesUsed = promotion.TimesUsed
            };
        }

        public async Task<PromotionDto> CreatePromotionAsync(PromotionCreateDto promotionCreateDto)
        {
            if (await _context.Promotions.AnyAsync(p => p.Code == promotionCreateDto.Code))
                throw new ArgumentException("Promotion code already exists");

            var promotion = new Promotion
            {
                Code = promotionCreateDto.Code,
                Description = promotionCreateDto.Description,
                DiscountType = promotionCreateDto.DiscountType,
                DiscountValue = promotionCreateDto.DiscountValue,
                StartDate = promotionCreateDto.StartDate,
                EndDate = promotionCreateDto.EndDate,
                UsageLimit = promotionCreateDto.UsageLimit,
                TimesUsed = 0
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return new PromotionDto
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                TimesUsed = promotion.TimesUsed
            };
        }

        public async Task<PromotionDto> UpdatePromotionAsync(int id, PromotionCreateDto promotionUpdateDto)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) throw new ArgumentException("Promotion not found");

            // Check if code is being changed and if it already exists
            if (promotion.Code != promotionUpdateDto.Code &&
                await _context.Promotions.AnyAsync(p => p.Code == promotionUpdateDto.Code && p.Id != id))
                throw new ArgumentException("Promotion code already exists");

            promotion.Code = promotionUpdateDto.Code;
            promotion.Description = promotionUpdateDto.Description;
            promotion.DiscountType = promotionUpdateDto.DiscountType;
            promotion.DiscountValue = promotionUpdateDto.DiscountValue;
            promotion.StartDate = promotionUpdateDto.StartDate;
            promotion.EndDate = promotionUpdateDto.EndDate;
            promotion.UsageLimit = promotionUpdateDto.UsageLimit;

            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();

            return new PromotionDto
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                TimesUsed = promotion.TimesUsed
            };
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) throw new ArgumentException("Promotion not found");

            _context.Promotions.Remove(promotion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, int userId)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code);

            if (promotion == null)
                return false;

            if (DateTime.UtcNow < promotion.StartDate || DateTime.UtcNow > promotion.EndDate)
                return false;

            if (promotion.UsageLimit > 0 && promotion.TimesUsed >= promotion.UsageLimit)
                return false;

            // Check if user has already used this promotion
            var hasUserUsedPromotion = await _context.PromotionUsages
                .AnyAsync(pu => pu.PromotionId == promotion.Id && pu.UserId == userId);

            if (hasUserUsedPromotion)
                return false;

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, int userId)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code);

            if (promotion == null)
                throw new ArgumentException("Promotion not found");

            if (!await ValidatePromotionAsync(code, orderAmount, userId))
                throw new InvalidOperationException("Promotion is not valid or has already been used by this user");

            decimal discount = 0;
            if (promotion.DiscountType == "Percentage")
            {
                discount = orderAmount * (promotion.DiscountValue / 100);
            }
            else if (promotion.DiscountType == "Fixed")
            {
                discount = promotion.DiscountValue;
            }

            // Cap discount to not exceed order amount
            if (discount > orderAmount)
                discount = orderAmount;

            // Record promotion usage
            var promotionUsage = new PromotionUsage
            {
                PromotionId = promotion.Id,
                UserId = userId,
                UsedAt = DateTime.UtcNow,
                OrderAmount = orderAmount,
                DiscountApplied = discount
            };

            _context.PromotionUsages.Add(promotionUsage);

            // Update usage count
            promotion.TimesUsed++;
            _context.Promotions.Update(promotion);

            await _context.SaveChangesAsync();

            return discount;
        }

        // Update other methods to include userId parameter where needed
        public async Task<bool> HasUserUsedPromotionAsync(int promotionId, int userId)
        {
            return await _context.PromotionUsages
                .AnyAsync(pu => pu.PromotionId == promotionId && pu.UserId == userId);
        }


    }
}

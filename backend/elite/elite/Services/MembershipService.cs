using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<MembershipService> _logger;

        public MembershipService(GymDbContext context, ILogger<MembershipService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MembershipDto> PurchaseMembershipAsync(PurchaseMembershipDto purchaseDto)
        {
            var user = await _context.Users.FindAsync(purchaseDto.UserId);
            if (user == null) throw new ArgumentException("User not found");

            // Check if user already has active membership
            var existingMembership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.UserId == purchaseDto.UserId && m.Status == "Active");

            if (existingMembership != null)
                throw new InvalidOperationException("User already has an active membership");

            // Get membership type details
            var membershipType = await _context.MembershipTypes
                .FirstOrDefaultAsync(mt => mt.Name == purchaseDto.Type && mt.IsActive);

            if (membershipType == null)
                throw new ArgumentException("Invalid membership type or type not available");

            var membership = new Membership
            {
                UserId = purchaseDto.UserId,
                Type = membershipType.Name,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(membershipType.DurationMonths),
                TotalHours = membershipType.TotalHours,
                RemainingHours = membershipType.TotalHours,
                Status = "Active",
                PricePaid = membershipType.Price
            };

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            return new MembershipDto
            {
                Id = membership.Id,
                Type = membership.Type,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                TotalHours = membership.TotalHours,
                RemainingHours = membership.RemainingHours,
                Status = membership.Status,
                PricePaid = membership.PricePaid // Add this line
            };
        }

        public async Task<MembershipDto> GetUserMembershipAsync(int userId)
        {
            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (membership == null) throw new ArgumentException("Membership not found");

            return new MembershipDto
            {
                Id = membership.Id,
                Type = membership.Type,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                TotalHours = membership.TotalHours,
                RemainingHours = membership.RemainingHours,
                Status = membership.Status
            };
        }

        public async Task<bool> CheckMembershipValidityAsync(int userId)
        {
            var membership = await _context.Memberships
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (membership == null) return false;
            if (membership.Status != "Active") return false;
            if (membership.EndDate < DateTime.UtcNow.Date)
            {
                membership.Status = "Expired";
                await _context.SaveChangesAsync();
                return false;
            }

            return true;
        }

        public async Task<bool> ExtendMembershipAsync(int membershipId, int additionalMonths)
        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new ArgumentException("Membership not found");

            membership.EndDate = membership.EndDate.AddMonths(additionalMonths);
            _context.Memberships.Update(membership);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpgradeMembershipAsync(int membershipId, string newType)
        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new ArgumentException("Membership not found");

            // FIX THIS PART - use decimal for hours
            decimal totalHours = newType switch
            {
                "Basic" => 10.0m,
                "Medium" => 25.0m,
                "VIP" => 50.0m,
                _ => throw new ArgumentException("Invalid membership type")
            };

            // Calculate prorated hours - use decimal for all calculations
            decimal daysUsed = (decimal)(DateTime.UtcNow - membership.StartDate).TotalDays;
            decimal totalDays = (decimal)(membership.EndDate - membership.StartDate).TotalDays;
            decimal hoursUsed = membership.TotalHours - membership.RemainingHours;
            decimal hoursPerDay = membership.TotalHours / totalDays;
            decimal hoursToDeduct = hoursPerDay * daysUsed;

            decimal newRemainingHours = totalHours - hoursToDeduct;
            if (newRemainingHours < 0) newRemainingHours = 0;

            membership.Type = newType;
            membership.TotalHours = totalHours;
            membership.RemainingHours = newRemainingHours;

            _context.Memberships.Update(membership);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

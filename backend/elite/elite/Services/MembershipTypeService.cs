// Services/MembershipTypeService.cs
using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elite.Services
{
    public class MembershipTypeService : IMembershipTypeService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<MembershipTypeService> _logger;

        public MembershipTypeService(GymDbContext context, ILogger<MembershipTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync()
        {
            return await _context.MembershipTypes
                .Where(mt => mt.IsActive)
                .Select(mt => new MembershipTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Description = mt.Description,
                    DurationMonths = mt.DurationMonths,
                    TotalHours = mt.TotalHours,
                    Price = mt.Price,
                    IsActive = mt.IsActive
                })
                .ToListAsync();
        }

        public async Task<MembershipTypeDto> GetMembershipTypeByIdAsync(int id)
        {
            var membershipType = await _context.MembershipTypes.FindAsync(id);
            if (membershipType == null) throw new ArgumentException("Membership type not found");

            return new MembershipTypeDto
            {
                Id = membershipType.Id,
                Name = membershipType.Name,
                Description = membershipType.Description,
                DurationMonths = membershipType.DurationMonths,
                TotalHours = membershipType.TotalHours,
                Price = membershipType.Price,
                IsActive = membershipType.IsActive
            };
        }

        public async Task<MembershipTypeDto> CreateMembershipTypeAsync(MembershipTypeCreateDto createDto)
        {
            if (await _context.MembershipTypes.AnyAsync(mt => mt.Name == createDto.Name))
                throw new ArgumentException("Membership type with this name already exists");

            var membershipType = new MembershipType
            {
                Name = createDto.Name,
                Description = createDto.Description,
                DurationMonths = createDto.DurationMonths,
                TotalHours = createDto.TotalHours,
                Price = createDto.Price,
                IsActive = true
            };

            _context.MembershipTypes.Add(membershipType);
            await _context.SaveChangesAsync();

            return new MembershipTypeDto
            {
                Id = membershipType.Id,
                Name = membershipType.Name,
                Description = membershipType.Description,
                DurationMonths = membershipType.DurationMonths,
                TotalHours = membershipType.TotalHours,
                Price = membershipType.Price,
                IsActive = membershipType.IsActive
            };
        }

        public async Task<MembershipTypeDto> UpdateMembershipTypeAsync(int id, MembershipTypeCreateDto updateDto)
        {
            var membershipType = await _context.MembershipTypes.FindAsync(id);
            if (membershipType == null) throw new ArgumentException("Membership type not found");

            // Check if name is being changed and if it conflicts with another type
            if (membershipType.Name != updateDto.Name &&
                await _context.MembershipTypes.AnyAsync(mt => mt.Name == updateDto.Name))
                throw new ArgumentException("Membership type with this name already exists");

            membershipType.Name = updateDto.Name;
            membershipType.Description = updateDto.Description;
            membershipType.DurationMonths = updateDto.DurationMonths;
            membershipType.TotalHours = updateDto.TotalHours;
            membershipType.Price = updateDto.Price;

            _context.MembershipTypes.Update(membershipType);
            await _context.SaveChangesAsync();

            return new MembershipTypeDto
            {
                Id = membershipType.Id,
                Name = membershipType.Name,
                Description = membershipType.Description,
                DurationMonths = membershipType.DurationMonths,
                TotalHours = membershipType.TotalHours,
                Price = membershipType.Price,
                IsActive = membershipType.IsActive
            };
        }

        public async Task<bool> DeleteMembershipTypeAsync(int id)
        {
            var membershipType = await _context.MembershipTypes.FindAsync(id);
            if (membershipType == null) throw new ArgumentException("Membership type not found");

            // Check if this membership type is being used by any active memberships
            var hasActiveMemberships = await _context.Memberships
                .AnyAsync(m => m.Type == membershipType.Name && m.Status == "Active");

            if (hasActiveMemberships)
                throw new InvalidOperationException("Cannot delete membership type with active memberships");

            _context.MembershipTypes.Remove(membershipType);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleMembershipTypeStatusAsync(int id, bool isActive)
        {
            var membershipType = await _context.MembershipTypes.FindAsync(id);
            if (membershipType == null) throw new ArgumentException("Membership type not found");

            membershipType.IsActive = isActive;
            _context.MembershipTypes.Update(membershipType);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
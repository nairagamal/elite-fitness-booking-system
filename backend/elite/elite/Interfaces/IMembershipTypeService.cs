// Interfaces/IMembershipTypeService.cs
using elite.DTOs;
using System.Threading.Tasks;

namespace elite.Interfaces
{
    public interface IMembershipTypeService
    {
        Task<IEnumerable<MembershipTypeDto>> GetAllMembershipTypesAsync();
        Task<MembershipTypeDto> GetMembershipTypeByIdAsync(int id);
        Task<MembershipTypeDto> CreateMembershipTypeAsync(MembershipTypeCreateDto createDto);
        Task<MembershipTypeDto> UpdateMembershipTypeAsync(int id, MembershipTypeCreateDto updateDto);
        Task<bool> DeleteMembershipTypeAsync(int id);
        Task<bool> ToggleMembershipTypeStatusAsync(int id, bool isActive);
    }
}
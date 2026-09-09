using elite.DTOs;

namespace elite.Interfaces
{
    public interface IMembershipService
    {
        Task<MembershipDto> PurchaseMembershipAsync(PurchaseMembershipDto purchaseDto);
        Task<MembershipDto> GetUserMembershipAsync(int userId);
        Task<bool> CheckMembershipValidityAsync(int userId);
        Task<bool> ExtendMembershipAsync(int membershipId, int additionalMonths);
        Task<bool> UpgradeMembershipAsync(int membershipId, string newType);
    }
}

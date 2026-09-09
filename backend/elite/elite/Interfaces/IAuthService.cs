using elite.DTOs;

namespace elite.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto);
        Task<UserResponseDto> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UserUpdateDto updateDto);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}

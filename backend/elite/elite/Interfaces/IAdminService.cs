using elite.DTOs;

namespace elite.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize);
        Task<UserResponseDto> GetUserDetailsAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        // In IAdminService.cs
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        // Interfaces/IAdminService.cs
        Task<BookingResponseDto> GetBookingDetailsAsync(int bookingId);

    }
}

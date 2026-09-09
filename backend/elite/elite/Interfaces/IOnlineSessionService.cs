using elite.DTOs;

namespace elite.Interfaces
{
    // Interfaces/IOnlineSessionService.cs
    public interface IOnlineSessionService
    {
        Task<IEnumerable<OnlineSessionDto>> GetAllSessionsAsync();
        Task<OnlineSessionDto> GetSessionByIdAsync(int id);
        Task<OnlineSessionDto> CreateSessionAsync(OnlineSessionCreateDto sessionCreateDto);
        Task<OnlineSessionDto> UpdateSessionAsync(int id, OnlineSessionCreateDto sessionUpdateDto);
        Task<bool> DeleteSessionAsync(int id);
        Task<IEnumerable<OnlineSessionScheduleDto>> GetSessionSchedulesAsync(int sessionId);
        Task<OnlineSessionScheduleDto> CreateSessionScheduleAsync(OnlineSessionScheduleCreateDto scheduleCreateDto);
        Task<bool> DeleteSessionScheduleAsync(int scheduleId);
    }

}

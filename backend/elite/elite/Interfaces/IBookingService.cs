// Interfaces/IBookingService.cs
using elite.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elite.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId);
        Task<BookingResponseDto> GetBookingDetailsAsync(int bookingId);
        Task<IEnumerable<BookingResponseDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
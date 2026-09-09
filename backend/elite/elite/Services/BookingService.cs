using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    public class BookingService : IBookingService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(GymDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Services/BookingService.cs
        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate user exists
                var user = await _context.Users
                    .Include(u => u.Membership)
                    .FirstOrDefaultAsync(u => u.Id == bookingDto.UserId);

                if (user == null)
                    throw new ArgumentException("User not found");

                // Validate membership
                if (user.Membership == null || user.Membership.Status != "Active")
                    throw new InvalidOperationException("User does not have an active membership");

                if (user.Membership.EndDate < DateTime.UtcNow.Date)
                {
                    user.Membership.Status = "Expired";
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException("Membership has expired");
                }

                decimal durationHours = 0;
                string className = string.Empty;
                string trainerName = string.Empty;
                DateTime startTime, endTime;
                string location = string.Empty;

                if (bookingDto.Type == "Class")
                {
                    // Get class schedule with related data
                    var schedule = await _context.ClassSchedules
                        .Include(cs => cs.Class)
                        .ThenInclude(c => c.Trainer)
                        .FirstOrDefaultAsync(cs => cs.Id == bookingDto.ScheduleId);

                    if (schedule == null)
                        throw new ArgumentException("Class schedule not found");

                    if (schedule.AvailableSlots <= 0)
                        throw new InvalidOperationException("No available slots for this class");

                    if (schedule.StartTime <= DateTime.UtcNow)
                        throw new InvalidOperationException("Class has already started");

                    // Convert duration from minutes to hours for membership calculation
                    durationHours = schedule.Class.Duration / 60.0m;
                    className = schedule.Class.Name;
                    trainerName = schedule.Class.Trainer.Name;
                    startTime = schedule.StartTime;
                    endTime = schedule.EndTime;
                    location = schedule.Location;

                    // Check for duplicate booking
                    var existingBooking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.UserId == bookingDto.UserId &&
                                                 b.Type == "Class" &&
                                                 b.ScheduleId == bookingDto.ScheduleId &&
                                                 b.Status == "Confirmed");

                    if (existingBooking != null)
                        throw new InvalidOperationException("User already booked this class");

                    // Decrement available slots
                    schedule.AvailableSlots--;
                    _context.ClassSchedules.Update(schedule);
                }
                else if (bookingDto.Type == "OnlineSession")
                {
                    // Get online session schedule with related data
                    var schedule = await _context.OnlineSessionSchedules
                        .Include(oss => oss.OnlineSession)
                        .ThenInclude(os => os.Trainer)
                        .FirstOrDefaultAsync(oss => oss.Id == bookingDto.ScheduleId);

                    if (schedule == null)
                        throw new ArgumentException("Online session schedule not found");

                    if (schedule.AvailableSlots <= 0)
                        throw new InvalidOperationException("No available slots for this session");

                    if (schedule.StartTime <= DateTime.UtcNow)
                        throw new InvalidOperationException("Session has already started");

                    // Convert duration from minutes to hours for membership calculation
                    durationHours = schedule.OnlineSession.Duration / 60.0m;
                    className = schedule.OnlineSession.Name;
                    trainerName = schedule.OnlineSession.Trainer.Name;
                    startTime = schedule.StartTime;
                    endTime = schedule.EndTime;

                    // Check for duplicate booking
                    var existingBooking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.UserId == bookingDto.UserId &&
                                                 b.Type == "OnlineSession" &&
                                                 b.ScheduleId == bookingDto.ScheduleId &&
                                                 b.Status == "Confirmed");

                    if (existingBooking != null)
                        throw new InvalidOperationException("User already booked this session");

                    // Decrement available slots
                    schedule.AvailableSlots--;
                    _context.OnlineSessionSchedules.Update(schedule);
                }
                else
                {
                    throw new ArgumentException("Invalid booking type. Must be 'Class' or 'OnlineSession'");
                }

                // Check if user has enough hours remaining
                if (user.Membership.RemainingHours < durationHours)
                    throw new InvalidOperationException($"Insufficient hours in membership. Required: {durationHours} hours, Available: {user.Membership.RemainingHours} hours");

                // Deduct hours from membership
                user.Membership.RemainingHours -= durationHours;
                _context.Memberships.Update(user.Membership);

                // Create the booking
                var booking = new Booking
                {
                    UserId = bookingDto.UserId,
                    Type = bookingDto.Type,
                    ScheduleId = bookingDto.ScheduleId,
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed",
                    HoursConsumed = durationHours
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return the booking details
                return new BookingResponseDto
                {
                    Id = booking.Id,
                    Type = booking.Type,
                    ScheduleId = booking.ScheduleId,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    HoursConsumed = booking.HoursConsumed,
                    ClassName = className,
                    TrainerName = trainerName,
                    StartTime = startTime,
                    EndTime = endTime,
                    Location = location
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating booking for user {UserId}, schedule {ScheduleId}",
                    bookingDto.UserId, bookingDto.ScheduleId);
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .ThenInclude(u => u.Membership)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null) throw new ArgumentException("Booking not found");
                if (booking.Status != "Confirmed") throw new InvalidOperationException("Booking cannot be cancelled");

                DateTime scheduleTime;
                decimal duration = booking.HoursConsumed;

                if (booking.Type == "Class")
                {
                    var schedule = await _context.ClassSchedules.FindAsync(booking.ScheduleId);
                    if (schedule == null) throw new ArgumentException("Class schedule not found");

                    scheduleTime = schedule.StartTime;

                    // Check cancellation policy (>24 hours)
                    if ((scheduleTime - DateTime.UtcNow).TotalHours > 24)
                    {
                        // Refund hours
                        booking.User.Membership.RemainingHours += duration; // FIX: Removed (int) cast
                        _context.Memberships.Update(booking.User.Membership);

                        // Increment available slots
                        schedule.AvailableSlots++;
                        _context.ClassSchedules.Update(schedule);
                    }
                }
                else
                {
                    var schedule = await _context.OnlineSessionSchedules.FindAsync(booking.ScheduleId);
                    if (schedule == null) throw new ArgumentException("Online session schedule not found");

                    scheduleTime = schedule.StartTime;

                    // Check cancellation policy (>24 hours)
                    if ((scheduleTime - DateTime.UtcNow).TotalHours > 24)
                    {
                        // Refund hours
                        booking.User.Membership.RemainingHours += duration; // FIX: Removed (int) cast
                        _context.Memberships.Update(booking.User.Membership);

                        // Increment available slots
                        schedule.AvailableSlots++;
                        _context.OnlineSessionSchedules.Update(schedule);
                    }
                }

                booking.Status = "Cancelled";
                _context.Bookings.Update(booking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling booking");
                throw;
            }
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Type = b.Type,
                    ScheduleId = b.ScheduleId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    HoursConsumed = b.HoursConsumed,
                    ClassName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Name)
                            .FirstOrDefault(),
                    TrainerName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Trainer.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Trainer.Name)
                            .FirstOrDefault(),
                    StartTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.StartTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.StartTime)
                            .FirstOrDefault(),
                    EndTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.EndTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.EndTime)
                            .FirstOrDefault(),
                    Location = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Location)
                            .FirstOrDefault()
                        : "Online"
                })
                .ToListAsync();

            return bookings;
        }

        // Services/BookingService.cs
        public async Task<BookingResponseDto> GetBookingDetailsAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User) // Include the User navigation property
                .Where(b => b.Id == bookingId)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Type = b.Type,
                    ScheduleId = b.ScheduleId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    HoursConsumed = b.HoursConsumed,
                    UserId = b.UserId,
                    UserName = b.User.Name,
                    ClassName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Name)
                            .FirstOrDefault(),
                    TrainerName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Trainer.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Trainer.Name)
                            .FirstOrDefault(),
                    StartTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.StartTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.StartTime)
                            .FirstOrDefault(),
                    EndTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.EndTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.EndTime)
                            .FirstOrDefault(),
                    Location = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Location)
                            .FirstOrDefault()
                        : "Online"
                })
                .FirstOrDefaultAsync();

            if (booking == null) throw new ArgumentException("Booking not found");
            return booking;
        }




        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Type = b.Type,
                    ScheduleId = b.ScheduleId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    HoursConsumed = b.HoursConsumed,
                    ClassName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Name)
                            .FirstOrDefault(),
                    TrainerName = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Class.Trainer.Name)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.OnlineSession.Trainer.Name)
                            .FirstOrDefault(),
                    StartTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.StartTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.StartTime)
                            .FirstOrDefault(),
                    EndTime = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.EndTime)
                            .FirstOrDefault()
                        : _context.OnlineSessionSchedules
                            .Where(oss => oss.Id == b.ScheduleId)
                            .Select(oss => oss.EndTime)
                            .FirstOrDefault(),
                    Location = b.Type == "Class"
                        ? _context.ClassSchedules
                            .Where(cs => cs.Id == b.ScheduleId)
                            .Select(cs => cs.Location)
                            .FirstOrDefault()
                        : "Online"
                })
                .ToListAsync();

            return bookings;
        }
    }
}
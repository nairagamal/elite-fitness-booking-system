using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    // Services/OnlineSessionService.cs
    public class OnlineSessionService : IOnlineSessionService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<OnlineSessionService> _logger;

        public OnlineSessionService(GymDbContext context, ILogger<OnlineSessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<OnlineSessionDto>> GetAllSessionsAsync()
        {
            return await _context.OnlineSessions
                .Include(os => os.Trainer)
                .Select(os => new OnlineSessionDto
                {
                    Id = os.Id,
                    Name = os.Name,
                    Description = os.Description,
                    Duration = os.Duration,
                    TrainerId = os.TrainerId,
                    TrainerName = os.Trainer.Name,
                    Price = os.Price
                })
                .ToListAsync();
        }

        public async Task<OnlineSessionDto> GetSessionByIdAsync(int id)
        {
            var session = await _context.OnlineSessions
                .Include(os => os.Trainer)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (session == null) throw new ArgumentException("Online session not found");

            return new OnlineSessionDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                Duration = session.Duration,
                TrainerId = session.TrainerId,
                TrainerName = session.Trainer.Name,
                Price = session.Price
            };
        }

        // Services/OnlineSessionService.cs
        public async Task<OnlineSessionDto> CreateSessionAsync(OnlineSessionCreateDto sessionCreateDto)
        {
            try
            {
                // Validate trainer exists
                var trainer = await _context.Trainers.FindAsync(sessionCreateDto.TrainerId);
                if (trainer == null)
                    throw new ArgumentException("Trainer not found");

                // Validate duration is in reasonable range (15 min to 5 hours)
                if (sessionCreateDto.Duration < 15 || sessionCreateDto.Duration > 300)
                    throw new ArgumentException("Duration must be between 15 and 300 minutes");

                // Check for duplicate session name
                if (await _context.OnlineSessions.AnyAsync(os => os.Name == sessionCreateDto.Name))
                    throw new ArgumentException("An online session with this name already exists");

                // Create the online session entity
                var session = new OnlineSession
                {
                    Name = sessionCreateDto.Name,
                    Description = sessionCreateDto.Description,
                    Duration = sessionCreateDto.Duration, // Duration in minutes
                    TrainerId = sessionCreateDto.TrainerId,
                    Price = sessionCreateDto.Price
                };

                _context.OnlineSessions.Add(session);
                await _context.SaveChangesAsync();

                // Return the created session details
                return new OnlineSessionDto
                {
                    Id = session.Id,
                    Name = session.Name,
                    Description = session.Description,
                    Duration = session.Duration,
                    TrainerId = session.TrainerId,
                    TrainerName = trainer.Name,
                    Price = session.Price
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating online session: {SessionName}", sessionCreateDto.Name);
                throw;
            }
        }

        public async Task<OnlineSessionDto> UpdateSessionAsync(int id, OnlineSessionCreateDto sessionUpdateDto)
        {
            var session = await _context.OnlineSessions.FindAsync(id);
            if (session == null) throw new ArgumentException("Online session not found");

            // Check if trainer exists
            var trainer = await _context.Trainers.FindAsync(sessionUpdateDto.TrainerId);
            if (trainer == null) throw new ArgumentException("Trainer not found");

            session.Name = sessionUpdateDto.Name;
            session.Description = sessionUpdateDto.Description;
            session.Duration = sessionUpdateDto.Duration;
            session.TrainerId = sessionUpdateDto.TrainerId;
            session.Price = sessionUpdateDto.Price;

            _context.OnlineSessions.Update(session);
            await _context.SaveChangesAsync();

            return new OnlineSessionDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                Duration = session.Duration,
                TrainerId = session.TrainerId,
                TrainerName = trainer.Name,
                Price = session.Price
            };
        }

        public async Task<bool> DeleteSessionAsync(int id)
        {
            var session = await _context.OnlineSessions.FindAsync(id);
            if (session == null) throw new ArgumentException("Online session not found");

            // Check if session has any schedules
            var hasSchedules = await _context.OnlineSessionSchedules.AnyAsync(oss => oss.OnlineSessionId == id);
            if (hasSchedules) throw new InvalidOperationException("Cannot delete session with existing schedules");

            _context.OnlineSessions.Remove(session);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<OnlineSessionScheduleDto>> GetSessionSchedulesAsync(int sessionId)
        {
            return await _context.OnlineSessionSchedules
                .Where(oss => oss.OnlineSessionId == sessionId)
                .Include(oss => oss.OnlineSession)
                .Select(oss => new OnlineSessionScheduleDto
                {
                    Id = oss.Id,
                    OnlineSessionId = oss.OnlineSessionId,
                    SessionName = oss.OnlineSession.Name,
                    StartTime = oss.StartTime,
                    EndTime = oss.EndTime,
                    AvailableSlots = oss.AvailableSlots,
                    MaxSlots = 20 // Fixed maximum for online sessions
                })
                .ToListAsync();
        }

        public async Task<OnlineSessionScheduleDto> CreateSessionScheduleAsync(OnlineSessionScheduleCreateDto scheduleCreateDto)
        {
            var session = await _context.OnlineSessions.FindAsync(scheduleCreateDto.OnlineSessionId);
            if (session == null) throw new ArgumentException("Online session not found");

            // Check if the schedule overlaps with existing schedules for the same trainer
            var overlappingSchedule = await _context.OnlineSessionSchedules
                .Where(oss => oss.OnlineSession.TrainerId == session.TrainerId)
                .Where(oss => oss.StartTime < scheduleCreateDto.EndTime && oss.EndTime > scheduleCreateDto.StartTime)
                .FirstOrDefaultAsync();

            if (overlappingSchedule != null)
                throw new InvalidOperationException("Schedule overlaps with existing session for this trainer");

            var schedule = new OnlineSessionSchedule
            {
                OnlineSessionId = scheduleCreateDto.OnlineSessionId,
                StartTime = scheduleCreateDto.StartTime,
                EndTime = scheduleCreateDto.EndTime,
                AvailableSlots = scheduleCreateDto.AvailableSlots
            };

            _context.OnlineSessionSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return new OnlineSessionScheduleDto
            {
                Id = schedule.Id,
                OnlineSessionId = schedule.OnlineSessionId,
                SessionName = session.Name,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                AvailableSlots = schedule.AvailableSlots,
                MaxSlots = 20
            };
        }

        public async Task<bool> DeleteSessionScheduleAsync(int scheduleId)
        {
            var schedule = await _context.OnlineSessionSchedules.FindAsync(scheduleId);
            if (schedule == null) throw new ArgumentException("Online session schedule not found");

            // Check if schedule has any bookings
            var hasBookings = await _context.Bookings.AnyAsync(b => b.ScheduleId == scheduleId && b.Type == "OnlineSession");
            if (hasBookings) throw new InvalidOperationException("Cannot delete schedule with existing bookings");

            _context.OnlineSessionSchedules.Remove(schedule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    // Services/AdminService.cs
    public class AdminService : IAdminService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(GymDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize)
        {
            return await _context.Users
                .Include(u => u.Membership)
                .Include(u => u.Bookings)
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    CreatedAt = u.CreatedAt,
                    Membership = u.Membership != null ? new MembershipDto
                    {
                        Id = u.Membership.Id,
                        Type = u.Membership.Type,
                        StartDate = u.Membership.StartDate,
                        EndDate = u.Membership.EndDate,
                        TotalHours = u.Membership.TotalHours,
                        RemainingHours = u.Membership.RemainingHours,
                        Status = u.Membership.Status
                    } : null
                })
                .ToListAsync();
        }

        public async Task<UserResponseDto> GetUserDetailsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Membership)
                .Include(u => u.Bookings)
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new ArgumentException("User not found");

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt,
                Membership = user.Membership != null ? new MembershipDto
                {
                    Id = user.Membership.Id,
                    Type = user.Membership.Type,
                    StartDate = user.Membership.StartDate,
                    EndDate = user.Membership.EndDate,
                    TotalHours = user.Membership.TotalHours,
                    RemainingHours = user.Membership.RemainingHours,
                    Status = user.Membership.Status
                } : null
            };
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            // For this implementation, we'll assume users are always active
            // In a real system, you might have an IsActive property on the User model
            return await Task.FromResult(true);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeMemberships = await _context.Memberships
                .CountAsync(m => m.Status == "Active" && m.EndDate >= DateTime.UtcNow);

            var totalBookingsToday = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == DateTime.UtcNow.Date && b.Status == "Confirmed");

            var revenueThisMonth = await _context.Orders
                .Where(o => o.OrderDate.Month == DateTime.UtcNow.Month &&
                           o.OrderDate.Year == DateTime.UtcNow.Year &&
                           o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            var lowStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity <= 5);

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                ActiveMemberships = activeMemberships,
                TotalBookingsToday = totalBookingsToday,
                RevenueThisMonth = revenueThisMonth,
                LowStockProducts = lowStockProducts
            };
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Bookings
                .Include(b => b.User) // Ensure User is included
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate <= endDate.Value);

            return await query
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Type = b.Type,
                    ScheduleId = b.ScheduleId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    HoursConsumed = b.HoursConsumed,
                    UserId = b.UserId,
                    UserName = b.User.Name, // Ensure this is set
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
        }



        // In AdminService.cs
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
        }

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
                    UserName = b.User.Name, // Ensure this is set
                    UserEmail = b.User.Email, // Add this
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

        public async Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == "Completed")
                .ToListAsync();

            var memberships = await _context.Memberships
                .Where(m => m.StartDate >= startDate && m.StartDate <= endDate && m.Status == "Active")
                .ToListAsync();

            var revenueByDate = new Dictionary<DateTime, RevenueReportDto>();

            // Initialize all dates in range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                revenueByDate[date] = new RevenueReportDto
                {
                    Date = date,
                    MembershipRevenue = 0,
                    ProductRevenue = 0,
                    TotalRevenue = 0,
                    BookingsCount = 0
                };
            }

            // Calculate product revenue from orders
            foreach (var order in orders)
            {
                var date = order.OrderDate.Date;
                if (revenueByDate.ContainsKey(date))
                {
                    revenueByDate[date].ProductRevenue += order.TotalAmount;
                    revenueByDate[date].TotalRevenue += order.TotalAmount;
                }
            }

            // Calculate membership revenue (simplified - assuming fixed prices)
            foreach (var membership in memberships)
            {
                var date = membership.StartDate.Date;
                if (revenueByDate.ContainsKey(date))
                {
                    decimal membershipPrice = membership.Type switch
                    {
                        "Basic" => 49.99m,
                        "Medium" => 99.99m,
                        "VIP" => 199.99m,
                        _ => 0
                    };

                    revenueByDate[date].MembershipRevenue += membershipPrice;
                    revenueByDate[date].TotalRevenue += membershipPrice;
                }
            }

            // Calculate bookings count
            var bookings = await _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate && b.Status == "Confirmed")
                .ToListAsync();

            foreach (var booking in bookings)
            {
                var date = booking.BookingDate.Date;
                if (revenueByDate.ContainsKey(date))
                {
                    revenueByDate[date].BookingsCount++;
                }
            }

            return revenueByDate.Values.OrderBy(r => r.Date).ToList();
        }
    }
}

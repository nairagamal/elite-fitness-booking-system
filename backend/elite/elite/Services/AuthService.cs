using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using elite.Utilities;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{


    public class AuthService : IAuthService
    {
        private readonly GymDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GymDbContext context, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new ArgumentException("Email already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Phone = registerDto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            // First try to find a user
            var user = await _context.Users
                .Include(u => u.Membership)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // If not found, try to find an admin
            if (user == null)
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

                if (admin != null && BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
                {
                    // Generate token for admin
                    var token = _jwtService.GenerateAdminToken(admin);

                    return new AuthResponseDto
                    {
                        Token = token,
                        User = new UserResponseDto
                        {
                            Id = admin.Id,
                            Name = admin.Username,
                            Email = admin.Email,
                            CreatedAt = DateTime.UtcNow,
                            // Admins don't have memberships
                            Membership = null
                        }
                    };
                }
            }

            // Original user login logic
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var userToken = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = userToken,
                User = new UserResponseDto
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
                }
            };
        }

        public async Task<UserResponseDto> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Membership)
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

        public async Task<bool> UpdateUserProfileAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            if (!string.IsNullOrEmpty(updateDto.Name))
                user.Name = updateDto.Name;

            if (!string.IsNullOrEmpty(updateDto.Phone))
                user.Phone = updateDto.Phone;

            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }

}

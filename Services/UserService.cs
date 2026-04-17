using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Oralink.DTOs.Admin;
using Oralink.Models;

namespace Oralink.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _env;

        public UserService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AuthenticateUser(string email, string password)
        {
            var user = await GetUserByEmail(email);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded ? user : null;
        }

        public async Task<List<UserResponseDto>> GetAllUsers()
        {
            return await _context.Users
                .Where(u => u.Role != "Admin")
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToListAsync();
        }

        public async Task<UserResponseDto?> CreateUser(string name, string email, string role, string password)
        {
            if (role == "Admin") throw new Exception("Admin cannot create another admin");

            var existingUser = await _context.Users.AnyAsync(u => u.Email == email.Trim().ToLower());
            if (existingUser) return null;

            var user = new User
            {
                Name = name,
                Email = email.Trim().ToLower(),
                Role = role,
                Department = "Periodontics",
                CreatedAt = DateTime.UtcNow
            };

            user.HashPassword = _passwordHasher.HashPassword(user, password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role, CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") };
        }

        public async Task<UserResponseDto?> UpdateUser(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email.Trim().ToLower() && u.Id != id);
            if (emailExists) throw new Exception("Email already exists");
            if (user.Role == "Admin") throw new Exception("Admin cannot be updated here");

            user.Name = dto.Name;
            user.Email = dto.Email.Trim().ToLower();
            user.Role = dto.Role;

            if (!string.IsNullOrEmpty(dto.Password))
                user.HashPassword = _passwordHasher.HashPassword(user, dto.Password);

            await _context.SaveChangesAsync();
            return new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role, CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") };
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            if (user.Role == "Admin") throw new Exception("Admin cannot be deleted");

            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetStatistics()
        {
            return new
            {
                TotalUsers = await _context.Users.CountAsync(u => u.Role != "Admin"),
                Students = await _context.Users.CountAsync(u => u.Role == "Student"),
                Doctors = await _context.Users.CountAsync(u => u.Role == "Doctor"),
                Supervisors = await _context.Users.CountAsync(u => u.Role == "Supervisor"),
                HODs = await _context.Users.CountAsync(u => u.Role == "Head of Department")
            };
        }

        public async Task<UserResponseDto?> UpdateAdminProfile(int adminId, AdminProfileUpdateDto dto)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != "Admin") return null;

            admin.Name = dto.Name;
            admin.Email = dto.Email.Trim().ToLower();

            if (!string.IsNullOrEmpty(dto.Password))
                admin.HashPassword = _passwordHasher.HashPassword(admin, dto.Password);

            await _context.SaveChangesAsync();
            return new UserResponseDto { Id = admin.Id, Name = admin.Name, Email = admin.Email, Role = admin.Role, CreatedAt = admin.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") };
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
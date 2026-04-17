using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Oralink.Models;

namespace Oralink.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AuthenticateUser(string email, string password)
        {
            var user = await GetUserByEmail(email);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded
                ? user
                : null;
        }
    }
}
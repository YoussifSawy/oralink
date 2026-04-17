using Oralink.Models;

namespace Oralink.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateUser(string email, string password);
        Task<User> GetUserByEmail(string email);
    }
}
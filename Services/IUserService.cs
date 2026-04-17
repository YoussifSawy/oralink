using Oralink.DTOs;
using Oralink.DTOs.Admin;
using Oralink.Models;

namespace Oralink.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateUser(string email, string password);
        Task<User> GetUserByEmail(string email);
        Task<UserResponseDto?> UpdateAdminProfile(int adminId, AdminProfileUpdateDto dto);
        Task<List<UserResponseDto>> GetAllUsers();
        Task<UserResponseDto?> CreateUser(string name, string email, string role, string password);
        Task<UserResponseDto?> UpdateUser(int id, UserUpdateDto dto);
        Task<bool> DeleteUser(int id);
        Task<User?> GetUserById(int id);
        Task<object> GetStatistics();
    }
}
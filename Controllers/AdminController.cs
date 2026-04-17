using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oralink.DTOs;
using Oralink.DTOs.Admin;
using Oralink.Services;
using System.Security.Claims;

namespace Oralink.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public AdminController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (dto.Role == "Head of Department")
                return BadRequest(new { Message = "Head of Department account already exists and cannot be created again" });

            var user = await _userService.CreateUser(dto.Name, dto.Email, dto.Role, dto.Password);
            if (user == null) return BadRequest(new { Message = "Email already exists" });

            return Ok(new { Message = "User created successfully", user.Id, user.Name, user.Email, user.Role });
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var existingUser = await _userService.GetUserById(id);
            if (existingUser == null) return NotFound(new { Message = "User not found" });

            if (existingUser.Role == "Head of Department" && dto.Role != "Head of Department")
                return BadRequest(new { Message = "Cannot change role of Head of Department" });

            if (dto.Role == "Head of Department" && existingUser.Role != "Head of Department")
            {
                var hodExists = await _userService.GetAllUsers();
                if (hodExists.Any(u => u.Role == "Head of Department"))
                    return BadRequest(new { Message = "There is already a Head of Department account, cannot assign this role" });
            }

            var user = await _userService.UpdateUser(id, new UserUpdateDto
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = existingUser.Role == "Head of Department" ? "Head of Department" : dto.Role,
                Password = dto.Password
            });

            return Ok(new { Message = "User updated successfully", user });
        }

        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound(new { Message = "User not found" });
            if (user.Role == "Head of Department") return BadRequest(new { Message = "Head of Department account cannot be deleted" });

            var adminId = int.Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (id == adminId) return BadRequest(new { Message = "You cannot delete your own account" });

            var result = await _userService.DeleteUser(id);
            if (!result) return NotFound(new { Message = "User not found" });

            return Ok(new { Message = "User deleted successfully" });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _userService.GetStatistics();
            return Ok(stats);
        }

        [HttpPut("update-admin-profile")]
        public async Task<IActionResult> UpdateAdminProfile(AdminProfileUpdateDto dto)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _userService.UpdateAdminProfile(adminId, dto);
            if (result == null) return NotFound();

            return Ok(new { Message = "Profile updated successfully", User = result });
        }

        [HttpPost("create-users-excel")]
        public async Task<IActionResult> BulkCreateUsers([FromBody] BulkCreateUsersDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Users == null || !dto.Users.Any()) return BadRequest(new { Message = "Users list is empty" });

            var duplicateEmailsInRequest = dto.Users.GroupBy(u => u.Email.ToLower()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateEmailsInRequest.Any())
                return BadRequest(new { Message = "Duplicate emails found in request", Duplicates = duplicateEmailsInRequest });

            if (dto.Role == "Head of Department")
                return BadRequest(new { Message = "Head of Department account already exists and cannot be created again" });

            var usersCreated = new List<object>();
            var errors = new List<string>();

            foreach (var userItem in dto.Users)
            {
                try
                {
                    var user = await _userService.CreateUser(userItem.Name, userItem.Email.ToLower(), dto.Role, userItem.Password);
                    if (user == null)
                    {
                        errors.Add($"Email already exists: {userItem.Email}");
                        continue;
                    }
                    usersCreated.Add(new { user.Id, user.Name, user.Email, user.Role });
                }
                catch (Exception ex)
                {
                    errors.Add($"{userItem.Email}: {ex.Message}");
                }
            }

            return Ok(new { Message = "Bulk process completed", Summary = new { TotalCreated = usersCreated.Count, TotalErrors = errors.Count }, CreatedUsers = usersCreated, Errors = errors });
        }

        [HttpDelete("delete-all-students")]
        public async Task<IActionResult> DeleteAllStudents()
        {
            var students = await _userService.GetAllUsers();
            var studentList = students.Where(u => u.Role == "Student").ToList();

            if (!studentList.Any()) return NotFound(new { Message = "No students found" });


            foreach (var student in studentList)
                await _userService.DeleteUser(student.Id);

            return Ok(new { Message = $"{studentList.Count} students deleted successfully" });
        }
    }
}
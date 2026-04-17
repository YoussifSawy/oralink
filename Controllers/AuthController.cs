using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oralink.DTOs;
using Oralink.DTOs.Auth;
using Oralink.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Oralink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, JwtService jwtService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var user = await _userService.AuthenticateUser(request.Email, request.Password);

            if (user == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid email or password" });
            }

            if (user.Role == "Student" && !Regex.IsMatch(user.Email, @"@o6u\.edu\.eg$", RegexOptions.IgnoreCase))
            {
                return Unauthorized(new { Success = false, Message = "Students must use O6U email" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                success = true,
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var nameClaim = User.FindFirst(ClaimTypes.Name);
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            if (idClaim == null || nameClaim == null || emailClaim == null || roleClaim == null)
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            var user = new CurrentUserDto
            {
                Id = int.Parse(idClaim.Value),
                Name = nameClaim.Value,
                Email = emailClaim.Value,
                Role = roleClaim.Value
            };

            return Ok(user);
        }
    }
}
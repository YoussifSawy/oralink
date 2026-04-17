namespace Oralink.DTOs.Admin
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string CreatedAt { get; set; } = "";
    }
}
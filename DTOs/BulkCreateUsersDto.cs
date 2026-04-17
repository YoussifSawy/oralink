using System.ComponentModel.DataAnnotations;

namespace Oralink.DTOs.Admin
{
    public class BulkCreateUsersDto
    {
        [Required]
        [RegularExpression("^(Student|Doctor|Supervisor|Head of Department)$", ErrorMessage = "Invalid role")]
        public string Role { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Users list cannot be empty")]
        public List<BulkUserItemDto> Users { get; set; }
    }

    public class BulkUserItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@o6u\.edu\.eg$", ErrorMessage = "Email must end with @o6u.edu.eg")]
        public string Email { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }
    }
}
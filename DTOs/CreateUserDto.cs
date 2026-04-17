using System.ComponentModel.DataAnnotations;

namespace Oralink.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(Student|Doctor|Supervisor|Head of Department)$", ErrorMessage = "Role must be Student, Doctor, Supervisor, or Head of Department")]
        public string Role { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; }
    }
}
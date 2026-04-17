using System.ComponentModel.DataAnnotations;

namespace Oralink.DTOs.Admin
{
    public class AdminProfileUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [MinLength(8)]
        [MaxLength(100)]
        public string? Password { get; set; }
    }
}
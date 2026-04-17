using System.ComponentModel.DataAnnotations;

namespace Oralink.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string HashPassword { get; set; }
        public string Department { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
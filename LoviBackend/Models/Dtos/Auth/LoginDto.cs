using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Auth
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}

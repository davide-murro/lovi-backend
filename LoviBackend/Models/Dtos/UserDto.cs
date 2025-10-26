using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; } = null!;

        public string? NewPassword { get; set; }

        [EmailAddress]
        public string Email { get; set; } = null!;

        public bool? EmailConfirmed { get; set; }

        [Required]
        public string Name { get; set; } = null!;
    }
}

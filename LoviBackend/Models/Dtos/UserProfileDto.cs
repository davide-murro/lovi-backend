using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class UserProfileDto
    {
        [Required]
        public string Id { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Name { get; set; } = null!;

    }
}

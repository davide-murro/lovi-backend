using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Auth
{
    public class RoleDto
    {
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;
    }
}

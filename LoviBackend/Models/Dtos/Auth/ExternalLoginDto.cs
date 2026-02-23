using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Auth
{
    public class ExternalLoginDto
    {
        [Required]
        public string Provider { get; set; } = null!;

        [Required]
        public string AccessToken { get; set; } = null!;
    }
}

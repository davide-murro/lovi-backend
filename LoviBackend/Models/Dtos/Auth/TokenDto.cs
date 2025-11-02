using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Auth
{
    public class TokenDto
    {
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}

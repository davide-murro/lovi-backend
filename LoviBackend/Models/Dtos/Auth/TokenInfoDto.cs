using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos.Auth
{
    public class TokenInfoDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserName { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string RefreshToken { get; set; } = null!;

        [Required]
        public DateTime ExpiredAt { get; set; }

        public DateTime? RefreshedAt { get; set; }

        [Required]
        public string DeviceId { get; set; } = null!;
    }
}

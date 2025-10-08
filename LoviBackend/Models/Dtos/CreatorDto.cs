using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class CreatorDto
    {
        public int Id { get; set; }

        [Required]
        public string Nickname { get; set; } = null!;

        public string? Name { get; set; }

        public string? Surname { get; set; }
    }
}

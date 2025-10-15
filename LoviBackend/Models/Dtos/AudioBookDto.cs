using LoviBackend.Models.DbSets;
using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class AudioBookDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImage { get; set; }

        public string? Description { get; set; }

        public string? AudioUrl { get; set; }
        public IFormFile? Audio { get; set; }

        public ICollection<CreatorDto>? Readers { get; set; } = null!;

    }
}

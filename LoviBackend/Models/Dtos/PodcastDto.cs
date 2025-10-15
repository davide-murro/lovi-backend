using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class PodcastDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImage { get; set; }

        public string? Description { get; set; }

        public ICollection<PodcastEpisodeDto>? Episodes { get; set; } = null!;

        public ICollection<CreatorDto>? Voicers { get; set; } = null!;

    }
}

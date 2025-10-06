using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class PodcastEpisodeDto
    {
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? CoverImageUrl { get; set; }

        public string? Description { get; set; }

        public string AudioUrl { get; set; } = null!;

        public PodcastDto Podcast { get; set; } = null!;

    }
}

using LoviBackend.Models.DbSets;
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
        public IFormFile? CoverImage { get; set; }

        public string? Description { get; set; }

        public string? AudioUrl { get; set; }
        public IFormFile? Audio { get; set; }

        public int? PodcastId { get; set; }
        public PodcastDto? Podcast { get; set; }

        public ICollection<CreatorDto>? Voicers { get; set; } = null!;

    }
}

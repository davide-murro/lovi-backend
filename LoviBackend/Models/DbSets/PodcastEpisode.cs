using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class PodcastEpisode
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int Number {  get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        public string? CoverImagePath { get; set; }

        public string? Description { get; set; }

        public string AudioPath { get; set; } = null!;

        public int PodcastId { get; set; }
        [ForeignKey(nameof(PodcastId))]
        public Podcast Podcast { get; set; } = null!;

        public ICollection<PodcastEpisodeVoicer> Voicers { get; set; } = new List<PodcastEpisodeVoicer>();

    }
}

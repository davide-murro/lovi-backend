using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class PodcastEpisodeVoicer
    {
        public int PodcastEpisodeId { get; set; }
        [ForeignKey(nameof(PodcastEpisodeId))]
        public PodcastEpisode PodcastEpisode { get; set; } = null!;

        public int CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public Creator Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

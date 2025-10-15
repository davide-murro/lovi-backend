namespace LoviBackend.Models.DbSets
{
    public class PodcastEpisodeVoicer
    {
        public int PodcastEpisodeId { get; set; }
        public PodcastEpisode PodcastEpisode { get; set; } = null!;

        public int CreatorId { get; set; }
        public Creator Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

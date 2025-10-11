namespace LoviBackend.Models.DbSets
{
    public class PodcastVoicer
    {
        public int PodcastId { get; set; }
        public Podcast Podcast { get; set; } = null!;

        public int CreatorId { get; set; }
        public Creator Creator { get; set; } = null!;
    }
}

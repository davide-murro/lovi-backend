using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class Library
    {
        [Key]
        public int Id { get; set; }

        // User (always required)
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        // Podcast (optional)
        public int? PodcastId { get; set; }

        [ForeignKey(nameof(PodcastId))]
        public Podcast? Podcast { get; set; }

        // PodcastEpisode (optional)
        public int? PodcastEpisodeId { get; set; }

        [ForeignKey(nameof(PodcastEpisodeId))]
        public PodcastEpisode? PodcastEpisode { get; set; }

        // AudioBook
        public int? AudioBookId { get; set; }
        
        [ForeignKey(nameof(AudioBookId))]
        public AudioBook? AudioBook { get; set; }
    }
}

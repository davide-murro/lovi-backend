
namespace LoviBackend.Models.Dtos
{
    public class LibraryDto
    {
        public int Id { get; set; }

        public UserDto User { get; set; } = null!;

        public PodcastDto? Podcast { get; set; }

        public PodcastEpisodeDto? PodcastEpisode { get; set; }

        public AudioBookDto? AudioBook { get; set; }

    }
}

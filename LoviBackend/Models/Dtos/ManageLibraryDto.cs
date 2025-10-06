
namespace LoviBackend.Models.Dtos
{
    public class ManageLibraryDto
    {
        public int? Id { get; set; }

        public string? UserId { get; set; }

        public int? PodcastId { get; set; }

        public int? PodcastEpisodeId { get; set; }

        public int? AudioBookId { get; set; }

    }
}

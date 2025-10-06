using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.DbSets
{
    public class Podcast
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        public string? CoverImagePath { get; set; }

        public string? Description { get; set; }

        public ICollection<PodcastEpisode> Episodes { get; set; } = new List<PodcastEpisode>();
    }
}

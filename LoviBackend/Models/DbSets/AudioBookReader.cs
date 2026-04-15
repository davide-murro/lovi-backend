using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class AudioBookReader
    {
        public int AudioBookId { get; set; }
        [ForeignKey(nameof(AudioBookId))]
        public AudioBook AudioBook { get; set; } = null!;

        public int CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public Creator Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

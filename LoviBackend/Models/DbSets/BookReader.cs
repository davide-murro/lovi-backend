using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class BookReader
    {
        public int BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        public int CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public Creator Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

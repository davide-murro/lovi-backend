using System.ComponentModel.DataAnnotations.Schema;

namespace LoviBackend.Models.DbSets
{
    public class EBookWriter
    {
        public int EBookId { get; set; }
        [ForeignKey(nameof(EBookId))]
        public EBook EBook { get; set; } = null!;

        public int CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public Creator Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

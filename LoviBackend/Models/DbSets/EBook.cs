using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.DbSets
{
    public class EBook
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        public string? CoverImagePath { get; set; }

        public string? CoverImagePreviewPath { get; set; }

        public string? Description { get; set; }

        // Stored file path for the ebook file (e.g. EPUB)
        public string? FilePath { get; set; }

        public ICollection<EBookWriter> Writers { get; set; } = new List<EBookWriter>();
    }
}

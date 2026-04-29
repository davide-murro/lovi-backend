using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.DbSets
{
    public class Book
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

        // Audio file path (for audiobooks)
        public string? AudioPath { get; set; }

        // EBook file path (for ebooks)
        public string? FilePath { get; set; }

        // Readers (audiobook readers)
        public ICollection<BookReader> Readers { get; set; } = new List<BookReader>();

        // Writers (ebook writers)
        public ICollection<BookWriter> Writers { get; set; } = new List<BookWriter>();
    }
}

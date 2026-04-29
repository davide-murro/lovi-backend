using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? DataUrl { get; set; }

        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImage { get; set; }

        public string? CoverImagePreviewUrl { get; set; }
        public IFormFile? CoverImagePreview { get; set; }

        public string? Description { get; set; }

        // Audio file (for audiobooks)
        public string? AudioUrl { get; set; }
        public IFormFile? Audio { get; set; }

        // EBook file (for ebooks)
        public string? FileUrl { get; set; }
        public IFormFile? File { get; set; }

        public ICollection<CreatorDto>? Readers { get; set; } = null!;
        public ICollection<CreatorDto>? Writers { get; set; } = null!;
    }
}

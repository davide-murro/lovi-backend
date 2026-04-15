using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.Dtos
{
    public class EBookDto
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

        // Stored file path and public URL for ebook file (e.g. EPUB)
        public string? FileUrl { get; set; }
        public IFormFile? File { get; set; }

        public ICollection<CreatorDto>? Writers { get; set; } = null!;
    }
}

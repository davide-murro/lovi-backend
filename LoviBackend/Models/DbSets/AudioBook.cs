using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.DbSets
{
    public class AudioBook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        public string? CoverImagePath { get; set; }

        public string? Description { get; set; }

        public string AudioPath { get; set; } = null!;
    }
}

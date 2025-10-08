using System.ComponentModel.DataAnnotations;

namespace LoviBackend.Models.DbSets
{
    public class Creator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Nickname { get; set; } = null!;

        public string? Name { get; set; }

        public string? Surname { get; set; }
    }
}

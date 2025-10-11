using Microsoft.AspNetCore.Identity;

namespace LoviBackend.Models.DbSets
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LoggedInAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Name { get; set; } = null!;
    }
}

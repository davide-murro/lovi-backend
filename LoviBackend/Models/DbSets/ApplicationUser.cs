using Microsoft.AspNetCore.Identity;

namespace LoviBackend.Models.DbSets
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = null!;
    }
}

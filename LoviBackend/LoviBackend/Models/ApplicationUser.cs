using Microsoft.AspNetCore.Identity;

namespace LoviBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}

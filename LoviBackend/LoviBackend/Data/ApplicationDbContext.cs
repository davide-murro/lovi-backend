using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LoviBackend.Models;
using LoviBackend.Models.DbSets;

namespace LoviBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options) 
        { }


        public DbSet<TokenInfo> TokenInfos { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
    }
}

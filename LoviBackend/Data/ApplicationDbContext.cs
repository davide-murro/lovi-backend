using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LoviBackend.Models.DbSets;

namespace LoviBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options) 
        { }

        public DbSet<TokenInfo> TokenInfos { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }
        public DbSet<Library> Libraries { get; set; }

        public DbSet<AudioBook> AudioBooks { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Podcast table
            builder.Entity<Podcast>(entity =>
            {
                // Index on Name, unique
                entity.HasIndex(e => new { e.Name })
                      .IsUnique();
            });

            // Configure PodcastEpisode table
            builder.Entity<PodcastEpisode>(entity =>
            {
                // Index on PodcastId + Number, unique
                entity.HasIndex(e => new { e.PodcastId, e.Number })
                      .IsUnique();

                // Relationship to Podcast (required)
                entity.HasOne(e => e.Podcast)
                      .WithMany(e => e.Episodes)
                      .HasForeignKey(e => e.PodcastId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AudioBook table
            builder.Entity<AudioBook>(entity =>
            {
                // Index on Name, unique
                entity.HasIndex(e => new { e.Name })
                      .IsUnique();
            });

            // Configure Libraries table
            builder.Entity<Library>(entity =>
            {
                // Index on Name, unique
                entity.HasIndex(e => new { e.UserId, e.PodcastId, e.PodcastEpisodeId })
                      .IsUnique();
                entity.HasIndex(e => new { e.UserId, e.AudioBookId })
                      .IsUnique();

                // Relationship: User (required, no cascade delete)
                entity.HasOne(e => e.User)
                      .WithMany() // or .WithMany(u => u.Libraries) if you add navigation on ApplicationUser
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Podcast (optional, no required delete)
                entity.HasOne(e => e.Podcast)
                      .WithMany() // or .WithMany(p => p.Libraries)
                      .HasForeignKey(e => e.PodcastId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship: PodcastEpisode (optional, no required delete)
                entity.HasOne(e => e.PodcastEpisode)
                      .WithMany() // or .WithMany(e => e.Libraries)
                      .HasForeignKey(e => e.PodcastEpisodeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // If you later add AudioBook
                entity.HasOne(e => e.AudioBook)
                      .WithMany()
                      .HasForeignKey(e => e.AudioBookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

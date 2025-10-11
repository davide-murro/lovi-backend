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
        public DbSet<AudioBook> AudioBooks { get; set; } 
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Creator> Creators { get; set; }
        public DbSet<PodcastVoicer> PodcastVoicers { get; set; }
        public DbSet<PodcastEpisodeVoicer> PodcastEpisodeVoicers { get; set; }
        public DbSet<AudioBookReader> AudioBookReaders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Podcasts table
            builder.Entity<Podcast>(entity =>
            {
                // Index on Name, unique
                entity.HasIndex(e => new { e.Name })
                      .IsUnique();
            });

            // Configure PodcastEpisodes table
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

            // Configure AudioBooks table
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

            // Configure Creators table
            builder.Entity<Creator>(entity =>
            {
                // Index on Name, unique
                entity.HasIndex(e => new { e.Nickname })
                      .IsUnique();
            });

            // Configure PodcastVoicers table
            builder.Entity<PodcastVoicer>(entity =>
            {
                // Define composite primary key
                entity.HasKey(pv => new { pv.PodcastId, pv.CreatorId });

                // Relationship to Podcast
                entity.HasOne(pv => pv.Podcast)
                      .WithMany(pv => pv.Voicers) // Assuming no navigation property on Podcast yet
                      .HasForeignKey(pv => pv.PodcastId)
                      .OnDelete(DeleteBehavior.Cascade); // If podcast is deleted, remove voicers

                // Relationship to Creator
                entity.HasOne(pv => pv.Creator)
                      .WithMany()
                      .HasForeignKey(pv => pv.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict); // Keep creators if a podcast is deleted
            });

            // Configure PodcastEpisodeVoicers table
            builder.Entity<PodcastEpisodeVoicer>(entity =>
            {
                // Define composite primary key
                entity.HasKey(pe => new { pe.PodcastEpisodeId, pe.CreatorId });

                // Relationship to PodcastEpisode
                entity.HasOne(pe => pe.PodcastEpisode)
                      .WithMany(pe => pe.Voicers)
                      .HasForeignKey(pe => pe.PodcastEpisodeId)
                      .OnDelete(DeleteBehavior.Cascade); // If episode is deleted, remove voicers

                // Relationship to Creator
                entity.HasOne(pe => pe.Creator)
                      .WithMany()
                      .HasForeignKey(pe => pe.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict); // Keep creators if a podcast is deleted
            });

            // Configure AudioBookReaders table
            builder.Entity<AudioBookReader>(entity =>
            {
                // Define composite primary key
                entity.HasKey(ar => new { ar.AudioBookId, ar.CreatorId });

                // Relationship to AudioBook
                entity.HasOne(ar => ar.AudioBook)
                      .WithMany(ar => ar.Readers) // Assuming no navigation property on AudioBook yet
                      .HasForeignKey(ar => ar.AudioBookId)
                      .OnDelete(DeleteBehavior.Cascade); // If audiobook is deleted, remove readers

                // Relationship to Creator
                entity.HasOne(ar => ar.Creator)
                      .WithMany() // Assuming no navigation property on Creator yet
                      .HasForeignKey(ar => ar.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict); // Keep creators if an audiobook is deleted
            });
        }
    }
}

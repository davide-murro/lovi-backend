using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditLibraries3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId_AudioBookId",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_AudioBookId",
                table: "Libraries",
                columns: new[] { "UserId", "AudioBookId" },
                unique: true,
                filter: "[AudioBookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_PodcastId",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId" },
                unique: true,
                filter: "[PodcastId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId" },
                unique: true,
                filter: "[PodcastId] IS NOT NULL AND [PodcastEpisodeId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_PodcastId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId_AudioBookId",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId", "AudioBookId" },
                unique: true,
                filter: "[PodcastId] IS NOT NULL AND [PodcastEpisodeId] IS NOT NULL AND [AudioBookId] IS NOT NULL");
        }
    }
}

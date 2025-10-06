using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeUniqueFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PodcastEpisodes_PodcastId",
                table: "PodcastEpisodes");

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_Name",
                table: "Podcasts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_PodcastId_Number",
                table: "PodcastEpisodes",
                columns: new[] { "PodcastId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Podcasts_Name",
                table: "Podcasts");

            migrationBuilder.DropIndex(
                name: "IX_PodcastEpisodes_PodcastId_Number",
                table: "PodcastEpisodes");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_PodcastId",
                table: "PodcastEpisodes",
                column: "PodcastId");
        }
    }
}

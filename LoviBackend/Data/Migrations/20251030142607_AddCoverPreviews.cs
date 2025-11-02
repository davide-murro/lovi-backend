using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverPreviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImagePreviewPath",
                table: "Podcasts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePreviewPath",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePreviewPath",
                table: "AudioBooks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImagePreviewPath",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "CoverImagePreviewPath",
                table: "PodcastEpisodes");

            migrationBuilder.DropColumn(
                name: "CoverImagePreviewPath",
                table: "AudioBooks");
        }
    }
}

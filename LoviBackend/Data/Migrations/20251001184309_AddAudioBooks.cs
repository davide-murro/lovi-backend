using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PodcastEpisodes_Name",
                table: "PodcastEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries");

            migrationBuilder.AlterColumn<string>(
                name: "AudioPath",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioBookId",
                table: "Libraries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId", "AudioBookId" });

            migrationBuilder.CreateTable(
                name: "AudioBook",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioBook", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_Name",
                table: "Podcasts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_AudioBookId",
                table: "Libraries",
                column: "AudioBookId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioBook_Name",
                table: "AudioBook",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_AudioBook_AudioBookId",
                table: "Libraries",
                column: "AudioBookId",
                principalTable: "AudioBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_AudioBook_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "AudioBook");

            migrationBuilder.DropIndex(
                name: "IX_Podcasts_Name",
                table: "Podcasts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "AudioBookId",
                table: "Libraries");

            migrationBuilder.AlterColumn<string>(
                name: "AudioPath",
                table: "PodcastEpisodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_Name",
                table: "PodcastEpisodes",
                column: "Name",
                unique: true);
        }
    }
}

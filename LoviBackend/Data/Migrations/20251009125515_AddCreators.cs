using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Creators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nickname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AudioBookReaders",
                columns: table => new
                {
                    AudioBookId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioBookReaders", x => new { x.AudioBookId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_AudioBookReaders_AudioBooks_AudioBookId",
                        column: x => x.AudioBookId,
                        principalTable: "AudioBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AudioBookReaders_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodcastEpisodeVoicers",
                columns: table => new
                {
                    PodcastEpisodeId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEpisodeVoicers", x => new { x.PodcastEpisodeId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_PodcastEpisodeVoicers_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PodcastEpisodeVoicers_PodcastEpisodes_PodcastEpisodeId",
                        column: x => x.PodcastEpisodeId,
                        principalTable: "PodcastEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PodcastVoicers",
                columns: table => new
                {
                    PodcastId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastVoicers", x => new { x.PodcastId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_PodcastVoicers_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PodcastVoicers_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudioBookReaders_CreatorId",
                table: "AudioBookReaders",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Creators_Nickname",
                table: "Creators",
                column: "Nickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodeVoicers_CreatorId",
                table: "PodcastEpisodeVoicers",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastVoicers_CreatorId",
                table: "PodcastVoicers",
                column: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioBookReaders");

            migrationBuilder.DropTable(
                name: "PodcastEpisodeVoicers");

            migrationBuilder.DropTable(
                name: "PodcastVoicers");

            migrationBuilder.DropTable(
                name: "Creators");
        }
    }
}

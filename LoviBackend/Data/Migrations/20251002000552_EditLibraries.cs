using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditLibraries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_AudioBook_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AudioBook",
                table: "AudioBook");

            migrationBuilder.RenameTable(
                name: "AudioBook",
                newName: "AudioBooks");

            migrationBuilder.RenameIndex(
                name: "IX_AudioBook_Name",
                table: "AudioBooks",
                newName: "IX_AudioBooks_Name");

            migrationBuilder.AlterColumn<int>(
                name: "AudioBookId",
                table: "Libraries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PodcastEpisodeId",
                table: "Libraries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PodcastId",
                table: "Libraries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AudioBooks",
                table: "AudioBooks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId_AudioBookId",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId", "AudioBookId" },
                unique: true,
                filter: "[PodcastId] IS NOT NULL AND [PodcastEpisodeId] IS NOT NULL AND [AudioBookId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_AudioBooks_AudioBookId",
                table: "Libraries",
                column: "AudioBookId",
                principalTable: "AudioBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_AudioBooks_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_PodcastId_PodcastEpisodeId_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AudioBooks",
                table: "AudioBooks");

            migrationBuilder.RenameTable(
                name: "AudioBooks",
                newName: "AudioBook");

            migrationBuilder.RenameIndex(
                name: "IX_AudioBooks_Name",
                table: "AudioBook",
                newName: "IX_AudioBook_Name");

            migrationBuilder.AlterColumn<int>(
                name: "PodcastId",
                table: "Libraries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PodcastEpisodeId",
                table: "Libraries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AudioBookId",
                table: "Libraries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Libraries",
                table: "Libraries",
                columns: new[] { "UserId", "PodcastId", "PodcastEpisodeId", "AudioBookId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AudioBook",
                table: "AudioBook",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_AudioBook_AudioBookId",
                table: "Libraries",
                column: "AudioBookId",
                principalTable: "AudioBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

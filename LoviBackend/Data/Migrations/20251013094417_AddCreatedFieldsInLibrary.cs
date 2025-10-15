using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedFieldsInLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PodcastVoicers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PodcastEpisodeVoicers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AudioBookReaders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PodcastVoicers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PodcastEpisodeVoicers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AudioBookReaders");
        }
    }
}

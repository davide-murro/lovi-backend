using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EBookId",
                table: "Libraries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePreviewPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EBookWriters",
                columns: table => new
                {
                    EBookId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EBookWriters", x => new { x.EBookId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_EBookWriters_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EBookWriters_EBooks_EBookId",
                        column: x => x.EBookId,
                        principalTable: "EBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_EBookId",
                table: "Libraries",
                column: "EBookId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_EBookId",
                table: "Libraries",
                columns: new[] { "UserId", "EBookId" },
                unique: true,
                filter: "[EBookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EBooks_Name",
                table: "EBooks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EBookWriters_CreatorId",
                table: "EBookWriters",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_EBooks_EBookId",
                table: "Libraries",
                column: "EBookId",
                principalTable: "EBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_EBooks_EBookId",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "EBookWriters");

            migrationBuilder.DropTable(
                name: "EBooks");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_EBookId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_EBookId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "EBookId",
                table: "Libraries");
        }
    }
}

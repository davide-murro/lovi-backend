using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_AudioBooks_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_EBooks_EBookId",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "AudioBookReaders");

            migrationBuilder.DropTable(
                name: "EBookWriters");

            migrationBuilder.DropTable(
                name: "AudioBooks");

            migrationBuilder.DropTable(
                name: "EBooks");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_AudioBookId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_EBookId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "AudioBookId",
                table: "Libraries");

            migrationBuilder.RenameColumn(
                name: "EBookId",
                table: "Libraries",
                newName: "BookId");

            migrationBuilder.RenameIndex(
                name: "IX_Libraries_EBookId",
                table: "Libraries",
                newName: "IX_Libraries_BookId");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePreviewPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookReaders",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReaders", x => new { x.BookId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_BookReaders_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookReaders_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookWriters",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookWriters", x => new { x.BookId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_BookWriters_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookWriters_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_BookId",
                table: "Libraries",
                columns: new[] { "UserId", "BookId" },
                unique: true,
                filter: "[BookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookReaders_CreatorId",
                table: "BookReaders",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Name",
                table: "Books",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookWriters_CreatorId",
                table: "BookWriters",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_Books_BookId",
                table: "Libraries",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_Books_BookId",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "BookReaders");

            migrationBuilder.DropTable(
                name: "BookWriters");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_BookId",
                table: "Libraries");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "Libraries",
                newName: "EBookId");

            migrationBuilder.RenameIndex(
                name: "IX_Libraries_BookId",
                table: "Libraries",
                newName: "IX_Libraries_EBookId");

            migrationBuilder.AddColumn<int>(
                name: "AudioBookId",
                table: "Libraries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AudioBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudioPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePreviewPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePreviewPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AudioBookReaders",
                columns: table => new
                {
                    AudioBookId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_Libraries_AudioBookId",
                table: "Libraries",
                column: "AudioBookId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_AudioBookId",
                table: "Libraries",
                columns: new[] { "UserId", "AudioBookId" },
                unique: true,
                filter: "[AudioBookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_EBookId",
                table: "Libraries",
                columns: new[] { "UserId", "EBookId" },
                unique: true,
                filter: "[EBookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AudioBookReaders_CreatorId",
                table: "AudioBookReaders",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioBooks_Name",
                table: "AudioBooks",
                column: "Name",
                unique: true);

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
                name: "FK_Libraries_AudioBooks_AudioBookId",
                table: "Libraries",
                column: "AudioBookId",
                principalTable: "AudioBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_EBooks_EBookId",
                table: "Libraries",
                column: "EBookId",
                principalTable: "EBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoviBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TokenInfos",
                newName: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "TokenInfos",
                newName: "Name");
        }
    }
}

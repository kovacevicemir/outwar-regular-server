using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Outwar_regular_server.Migrations
{
    /// <inheritdoc />
    public partial class AddLockToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "Items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locked",
                table: "Items");
        }
    }
}

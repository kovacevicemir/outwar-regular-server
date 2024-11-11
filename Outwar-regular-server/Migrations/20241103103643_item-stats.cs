using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Outwar_regular_server.Migrations
{
    /// <inheritdoc />
    public partial class itemstats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "SetBonus",
                table: "Items",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int[]>(
                name: "Stats",
                table: "Items",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Stats",
                table: "Items");
        }
    }
}

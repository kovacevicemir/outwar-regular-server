using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Outwar_regular_server.Migrations
{
    /// <inheritdoc />
    public partial class addskillstouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "Skills",
                table: "Users",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Users");
        }
    }
}

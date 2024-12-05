using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Outwar_regular_server.Migrations
{
    /// <inheritdoc />
    public partial class addskillspointstouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkillPoints",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillPoints",
                table: "Users");
        }
    }
}

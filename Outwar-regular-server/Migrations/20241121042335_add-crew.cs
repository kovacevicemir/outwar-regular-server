using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Outwar_regular_server.Migrations
{
    /// <inheritdoc />
    public partial class addcrew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CrewId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewId",
                table: "Items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Crews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CrewLeaderId = table.Column<int>(type: "integer", nullable: false),
                    CrewUpgrades = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CrewId",
                table: "Users",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CrewId",
                table: "Items",
                column: "CrewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Crews_CrewId",
                table: "Items",
                column: "CrewId",
                principalTable: "Crews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Crews_CrewId",
                table: "Users",
                column: "CrewId",
                principalTable: "Crews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Crews_CrewId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Crews_CrewId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Crews");

            migrationBuilder.DropIndex(
                name: "IX_Users_CrewId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Items_CrewId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CrewId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CrewId",
                table: "Items");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class GhostExactCalls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GhostAttemptPips",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GhostAttemptQuantity",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GhostAttemptsLeft",
                table: "Players",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhostAttemptPips",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GhostAttemptQuantity",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GhostAttemptsLeft",
                table: "Players");
        }
    }
}

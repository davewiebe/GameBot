using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class StatusAndGhost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanCallExactToJoinAgain",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "StatusMessage",
                table: "Games",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanCallExactToJoinAgain",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "Games");
        }
    }
}

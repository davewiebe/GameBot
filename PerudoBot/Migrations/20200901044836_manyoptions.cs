using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class manyoptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanCallExactAnytime",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCallLiarAnytime",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ExactCall",
                table: "Games",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoundStartPlayerId",
                table: "Games",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanCallExactAnytime",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CanCallLiarAnytime",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ExactCall",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RoundStartPlayerId",
                table: "Games");
        }
    }
}

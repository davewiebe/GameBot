using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class AddEloRatingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EloRatingStandard",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EloRatingSuddenDeath",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EloRatingVariable",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EloRatingChange",
                table: "GamePlayers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EloRatingStandard",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "EloRatingSuddenDeath",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "EloRatingVariable",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "EloRatingChange",
                table: "GamePlayers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class AddDurationFieldsToGameRoundAndAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Rounds",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Actions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Actions");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class reversePenaltyAndTerminatorMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PenaltyGainDice",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TerminatorMode",
                table: "Games",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyGainDice",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TerminatorMode",
                table: "Games");
        }
    }
}

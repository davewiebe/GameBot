using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class DiceCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurnOrder",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDice",
                table: "Games",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnOrder",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "NumberOfDice",
                table: "Games");
        }
    }
}

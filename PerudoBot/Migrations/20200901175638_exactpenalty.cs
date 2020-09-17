using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class exactpenalty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExactCall",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "ExactCallBonus",
                table: "Games",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExactCallPenalty",
                table: "Games",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExactCallBonus",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ExactCallPenalty",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "ExactCall",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

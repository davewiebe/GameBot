using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class morerattles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tauntrattle",
                table: "Rattles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Winrattle",
                table: "Rattles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tauntrattle",
                table: "Rattles");

            migrationBuilder.DropColumn(
                name: "Winrattle",
                table: "Rattles");
        }
    }
}

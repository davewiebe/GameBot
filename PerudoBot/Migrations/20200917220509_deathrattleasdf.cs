using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class deathrattleasdf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gif",
                table: "Rattles", newName: "Deathrattle");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deathrattle",
                table: "Rattles");

            migrationBuilder.AddColumn<string>(
                name: "Gif",
                table: "Rattles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

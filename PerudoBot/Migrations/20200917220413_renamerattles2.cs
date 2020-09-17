using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class renamerattles2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Deathrattles",
                table: "Deathrattles");

            migrationBuilder.RenameTable(
                name: "Deathrattles",
                newName: "Rattles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rattles",
                table: "Rattles",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Rattles",
                table: "Rattles");

            migrationBuilder.RenameTable(
                name: "Rattles",
                newName: "Deathrattles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deathrattles",
                table: "Deathrattles",
                column: "Id");
        }
    }
}

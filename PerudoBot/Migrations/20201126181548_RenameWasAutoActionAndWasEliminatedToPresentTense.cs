using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class RenameWasAutoActionAndWasEliminatedToPresentTense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasEliminated",
                newName: "IsEliminated",
                table: "GamePlayerRounds");

            migrationBuilder.RenameColumn(
                name: "WasAutoAction",
                newName: "IsAutoAction",
                table: "Actions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                newName: "WasEliminated",
                name: "IsEliminated",
                table: "GamePlayerRounds");

            migrationBuilder.RenameColumn(
                newName: "WasAutoAction",
                name: "IsAutoAction",
                table: "Actions");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class RenamePlayersToGamePlayers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_Players_GameId");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Players_PlayerId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Games_GameId",
                table: "Players ");

            migrationBuilder.DropPrimaryKey(
                table: "Players",
                name: "PK_Players");

            migrationBuilder.RenameTable(
                name: "Players",
                newName: "GamePlayers");

            migrationBuilder.AddPrimaryKey(
                table: "GamePlayers",
                name: "PK_GamePlayers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                table: "GamePlayers",
                name: "FK_GamePlayers_Games_GameId",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayers_PlayerId",
                table: "Actions",
                column: "PlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
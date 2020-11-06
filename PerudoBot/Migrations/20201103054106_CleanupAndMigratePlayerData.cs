using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class CleanupAndMigratePlayerData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""GamePlayers"" as gp
SET ""PlayerId"" = (SELECT ""Id"" FROM ""Players""
WHERE ""Username"" = gp.""Username"")");

            migrationBuilder.Sql(@"UPDATE ""Players"" SET ""Nickname"" = ""Username""
WHERE ""Nickname"" is null or ""Nickname"" = ''");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayers_PlayerId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers");

            migrationBuilder.DropIndex(
                name: "IX_Actions_PlayerId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "GamePlayers");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "GamePlayers");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                newName: "GamePlayerId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "GamePlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "GamePlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_GamePlayerId",
                table: "Actions",
                column: "GamePlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions",
                column: "GamePlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers");

            migrationBuilder.DropIndex(
                name: "IX_Actions_GamePlayerId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "GamePlayerId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "GamePlayers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "GamePlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "GamePlayers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "Actions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_PlayerId",
                table: "Actions",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayers_PlayerId",
                table: "Actions",
                column: "PlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Players_PlayerId",
                table: "GamePlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class CleanupExtraFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "Actions",
                newName: "GamePlayerId");

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

            migrationBuilder.RenameColumn(
                name: "GamePlayerId",
                newName: "PlayerId",
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
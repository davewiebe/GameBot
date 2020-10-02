using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class FixingStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Games_GameId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Rounds_RoundId",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_GameId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "RoundId",
                table: "Actions",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Rounds_RoundId",
                table: "Actions",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Rounds_RoundId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "RoundId",
                table: "Actions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Actions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_GameId",
                table: "Actions",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Games_GameId",
                table: "Actions",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Rounds_RoundId",
                table: "Actions",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

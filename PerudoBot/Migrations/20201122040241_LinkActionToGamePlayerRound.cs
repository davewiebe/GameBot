using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class LinkActionToGamePlayerRound : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GamePlayerRoundId",
                table: "Actions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_GamePlayerRoundId",
                table: "Actions",
                column: "GamePlayerRoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions",
                column: "GamePlayerRoundId",
                principalTable: "GamePlayerRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
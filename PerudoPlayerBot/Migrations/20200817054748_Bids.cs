using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoPlayerBot.Migrations
{
    public partial class Bids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerDice_Players_PlayerId",
                table: "PlayerDice");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerDice_Rounds_RoundId",
                table: "PlayerDice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerDice",
                table: "PlayerDice");

            migrationBuilder.RenameTable(
                name: "PlayerDice",
                newName: "PlayerRound");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerDice_RoundId",
                table: "PlayerRound",
                newName: "IX_PlayerRound_RoundId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerDice_PlayerId",
                table: "PlayerRound",
                newName: "IX_PlayerRound_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerRound",
                table: "PlayerRound",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Pips = table.Column<int>(nullable: false),
                    IsExact = table.Column<bool>(nullable: false),
                    IsLiar = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bids_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameId",
                table: "Rounds",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_PlayerId",
                table: "Bids",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_RoundId",
                table: "Bids",
                column: "RoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerRound_Players_PlayerId",
                table: "PlayerRound",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerRound_Rounds_RoundId",
                table: "PlayerRound",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Games_GameId",
                table: "Rounds",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerRound_Players_PlayerId",
                table: "PlayerRound");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerRound_Rounds_RoundId",
                table: "PlayerRound");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Games_GameId",
                table: "Rounds");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_GameId",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerRound",
                table: "PlayerRound");

            migrationBuilder.RenameTable(
                name: "PlayerRound",
                newName: "PlayerDice");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerRound_RoundId",
                table: "PlayerDice",
                newName: "IX_PlayerDice_RoundId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerRound_PlayerId",
                table: "PlayerDice",
                newName: "IX_PlayerDice_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerDice",
                table: "PlayerDice",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerDice_Players_PlayerId",
                table: "PlayerDice",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerDice_Rounds_RoundId",
                table: "PlayerDice",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class AddRoundPlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Place",
                table: "GamePlayers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoundPlayer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoundId = table.Column<int>(nullable: false),
                    GamePlayerId = table.Column<int>(nullable: false),
                    NumberOfDice = table.Column<int>(nullable: false),
                    Dice = table.Column<string>(nullable: true),
                    TurnOrder = table.Column<int>(nullable: false),
                    IsGhost = table.Column<bool>(nullable: false),
                    WasEliminated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoundPlayer_GamePlayers_GamePlayerId",
                        column: x => x.GamePlayerId,
                        principalTable: "GamePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoundPlayer_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundPlayer_GamePlayerId",
                table: "RoundPlayer",
                column: "GamePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundPlayer_RoundId",
                table: "RoundPlayer",
                column: "RoundId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoundPlayer");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "GamePlayers");
        }
    }
}

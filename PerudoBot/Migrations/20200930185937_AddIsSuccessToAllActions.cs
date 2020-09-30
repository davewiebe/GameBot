using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class AddIsSuccessToAllActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OldBids");

            migrationBuilder.DropTable(
                name: "TauntLogs");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSuccess",
                table: "Actions",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsSuccess",
                table: "Actions",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.CreateTable(
                name: "OldBids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Call = table.Column<string>(type: "text", nullable: true),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Pips = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OldBids_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TauntLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    TaunteePlayerId = table.Column<int>(type: "integer", nullable: false),
                    TaunterPlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TauntLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TauntLogs_Players_TaunteePlayerId",
                        column: x => x.TaunteePlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TauntLogs_Players_TaunterPlayerId",
                        column: x => x.TaunterPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OldBids_PlayerId",
                table: "OldBids",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TauntLogs_TaunteePlayerId",
                table: "TauntLogs",
                column: "TaunteePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TauntLogs_TaunterPlayerId",
                table: "TauntLogs",
                column: "TaunterPlayerId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class Actions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Players_PlayerId",
                table: "Bids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bids",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "Call",
                table: "Bids");

            migrationBuilder.RenameTable(
                name: "Bids",
                newName: "Actions");

            migrationBuilder.RenameIndex(
                name: "IX_Bids_PlayerId",
                table: "Actions",
                newName: "IX_Actions_PlayerId");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Actions",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Pips",
                table: "Actions",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "Actions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentActionId",
                table: "Actions",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "Actions",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OldBids",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Pips = table.Column<int>(nullable: false),
                    Call = table.Column<string>(nullable: true)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(nullable: false),
                    TaunterPlayerId = table.Column<int>(nullable: false),
                    TaunteePlayerId = table.Column<int>(nullable: false)
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
                name: "IX_Actions_GameId",
                table: "Actions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ParentActionId",
                table: "Actions",
                column: "ParentActionId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Games_GameId",
                table: "Actions",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Actions_ParentActionId",
                table: "Actions",
                column: "ParentActionId",
                principalTable: "Actions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Players_PlayerId",
                table: "Actions",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Games_GameId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Actions_ParentActionId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Players_PlayerId",
                table: "Actions");

            migrationBuilder.DropTable(
                name: "OldBids");

            migrationBuilder.DropTable(
                name: "TauntLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_GameId",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_ParentActionId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "ParentActionId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "Actions");

            migrationBuilder.RenameTable(
                name: "Actions",
                newName: "Bids");

            migrationBuilder.RenameIndex(
                name: "IX_Actions_PlayerId",
                table: "Bids",
                newName: "IX_Bids_PlayerId");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Bids",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Pips",
                table: "Bids",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Call",
                table: "Bids",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bids",
                table: "Bids",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Players_PlayerId",
                table: "Bids",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

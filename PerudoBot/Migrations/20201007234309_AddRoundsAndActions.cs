using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class AddRoundsAndActions : Migration
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

            migrationBuilder.DropColumn(
                name: "GameId",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsOutOfTurn",
                table: "Actions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "Actions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentActionId",
                table: "Actions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundId",
                table: "Actions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(nullable: false),
                    RoundNumber = table.Column<int>(nullable: false),
                    StartingPlayerId = table.Column<int>(nullable: false),
                    RoundType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ParentActionId",
                table: "Actions",
                column: "ParentActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_RoundId",
                table: "Actions",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameId",
                table: "Rounds",
                column: "GameId");

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
                name: "FK_Actions_Actions_ParentActionId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Players_PlayerId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Rounds_RoundId",
                table: "Actions");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_ParentActionId",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_RoundId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsOutOfTurn",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "ParentActionId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "RoundId",
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

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Bids",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

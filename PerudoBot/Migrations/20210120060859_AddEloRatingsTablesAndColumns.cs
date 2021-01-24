using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PerudoBot.Migrations
{
    public partial class AddEloRatingsTablesAndColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostGameEloRating",
                table: "GamePlayers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreGameEloRating",
                table: "GamePlayers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EloRatings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    GameMode = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EloRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EloRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EloRatings_PlayerId",
                table: "EloRatings",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EloRatings");

            migrationBuilder.DropColumn(
                name: "PostGameEloRating",
                table: "GamePlayers");

            migrationBuilder.DropColumn(
                name: "PreGameEloRating",
                table: "GamePlayers");
        }
    }
}

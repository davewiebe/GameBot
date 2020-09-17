using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace KarmaBot.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Karma",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Server = table.Column<decimal>(nullable: false),
                    Thing = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    FromUserId = table.Column<decimal>(nullable: false),
                    GivenOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karma", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeyPhrase",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyPhrase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Points = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phrase",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyPhraseId = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phrase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phrase_KeyPhrase_KeyPhraseId",
                        column: x => x.KeyPhraseId,
                        principalTable: "KeyPhrase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Phrase_KeyPhraseId",
                table: "Phrase",
                column: "KeyPhraseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Karma");

            migrationBuilder.DropTable(
                name: "Phrase");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "KeyPhrase");
        }
    }
}

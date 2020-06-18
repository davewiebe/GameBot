using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class AddedKarmaTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Karma",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Thing = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    FromUserId = table.Column<decimal>(nullable: false),
                    GivenOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karma", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Karma");
        }
    }
}

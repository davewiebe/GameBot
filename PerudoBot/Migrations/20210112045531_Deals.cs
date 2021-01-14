using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class Deals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasActiveDeal",
                table: "GamePlayers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<int>>(
                name: "UserDealIds",
                table: "GamePlayers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasActiveDeal",
                table: "GamePlayers");

            migrationBuilder.DropColumn(
                name: "UserDealIds",
                table: "GamePlayers");
        }
    }
}

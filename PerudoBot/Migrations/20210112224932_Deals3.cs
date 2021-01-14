using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class Deals3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserDealIds",
                table: "GamePlayers",
                nullable: true,
                oldClrType: typeof(int[]),
                oldType: "integer[]",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "UserDealIds",
                table: "GamePlayers",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class palifico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Games",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "Games",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsRanked",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NextRoundIsPalifico",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Palifico",
                table: "Games",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Winner",
                table: "Games",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsRanked",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "NextRoundIsPalifico",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Palifico",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Winner",
                table: "Games");
        }
    }
}

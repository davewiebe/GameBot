using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class AddMoreTimestampsToGameRoundAndActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinished",
                table: "Rounds",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStarted",
                table: "Rounds",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStarted",
                table: "Games",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Penalty",
                table: "GamePlayerRounds",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "Actions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"update ""Games"" Set ""DateStarted"" = ""DateCreated""
where ""DateStarted"" = '0001-01-01 00:00:00'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFinished",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DateStarted",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DateStarted",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Penalty",
                table: "GamePlayerRounds");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "Actions");
        }
    }
}
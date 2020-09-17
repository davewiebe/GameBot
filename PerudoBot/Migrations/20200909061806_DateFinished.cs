using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class DateFinished : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinished",
                table: "Games",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFinished",
                table: "Games");
        }
    }
}

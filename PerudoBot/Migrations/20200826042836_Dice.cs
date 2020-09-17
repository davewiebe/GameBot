using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class Dice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Die1",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Die2",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Die3",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Die4",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Die5",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "Dice",
                table: "Players",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dice",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "Die1",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die2",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die3",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die4",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die5",
                table: "Players",
                type: "int",
                nullable: true);
        }
    }
}

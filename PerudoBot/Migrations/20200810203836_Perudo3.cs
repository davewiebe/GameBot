using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class Perudo3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dice",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "Die1",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die2",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die3",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die4",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Die5",
                table: "Players",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDice",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Bids",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "NumberOfDice",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Bids");

            migrationBuilder.AddColumn<string>(
                name: "Dice",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

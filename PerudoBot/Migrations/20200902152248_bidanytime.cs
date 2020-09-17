using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class bidanytime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanBidAnytime",
                table: "Games",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanBidAnytime",
                table: "Games");
        }
    }
}

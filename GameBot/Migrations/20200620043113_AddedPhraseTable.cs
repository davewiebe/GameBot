using Microsoft.EntityFrameworkCore.Migrations;

namespace GameBot.Migrations
{
    public partial class AddedPhraseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyPhrase",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyPhrase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phrase",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                name: "Phrase");

            migrationBuilder.DropTable(
                name: "KeyPhrase");
        }
    }
}

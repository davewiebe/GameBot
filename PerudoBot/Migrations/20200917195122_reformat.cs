using Microsoft.EntityFrameworkCore.Migrations;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Linq;

namespace PerudoBot.Migrations
{
    public partial class reformat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var db = new GameBotDbContext();
            var notes = db.Notes.AsQueryable().Where(x => true).ToList();
            foreach (var note in notes)
            {
                note.Text = note.Text.StripSpecialCharacters();
            }
            db.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

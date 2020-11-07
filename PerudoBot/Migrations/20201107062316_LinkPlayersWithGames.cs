using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class LinkPlayersWithGames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""GamePlayers""
SET     ""PlayerId"" = (
	SELECT p.""Id""
	FROM ""Players"" p
	LEFT JOIN ""GamePlayers"" gp
	ON gp.""Username"" = p.""Username""
	LEFT JOIN ""Games"" g
	ON g.""Id"" = gp.""GameId""
	WHERE g.""GuildId"" = p.""GuildId""
	AND ""GamePlayers"".""Id"" = gp.""Id""
)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
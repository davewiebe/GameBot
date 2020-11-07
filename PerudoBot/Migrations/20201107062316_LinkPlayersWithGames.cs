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

            migrationBuilder.Sql(@"UPDATE ""Players"" SET ""Nickname"" = ""Username""
WHERE ""Nickname"" is null or ""Nickname"" = ''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Migrations
{
    public partial class PopulateNewPlayersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO ""Players"" (""Username"", ""IsBot"", ""GuildId"", ""UserId"")
SELECT DISTINCT ""Username"", ""IsBot"", g.""GuildId"", 0
FROM ""GamePlayers"" gp
LEFT JOIN ""Games"" g
ON g.""Id"" = gp.""GameId""
WHERE gp.""Username"" NOT IN
(SELECT ""Username"" From ""Players"")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
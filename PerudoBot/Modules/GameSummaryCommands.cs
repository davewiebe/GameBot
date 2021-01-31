using Discord;
using Discord.Commands;
using PerudoBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Alias("summary")]
        [Command("GameSummary")]
        private async Task GetGameSummaryAsync(int gameId)
        {
            var game = await new PerudoGameSummaryService(_db)
                .GetGameSummaryAsync(gameId);

            if (game == null)
            {
                await SendMessageAsync("Game not found.");
                return;
            }

            if (game.GameState != GameState.Finished)
            {
                await SendMessageAsync("Game summaries only generated for finished games.");
                return;
            }

            var gamePlayersTable = "";
            if (game.GamePlayers.Any(gp => gp.Rank == null))
            {
                gamePlayersTable = "No ranking or rating data available.";
            }
            else
            {
                //create player table
                var rankPadding = 4;
                var usernamePadding = game.GamePlayers.Select(gp => gp.Nickname.Length).Max() + 2;
                var eloRatingPadding = 8;
                var eloChangePadding = 5;

                gamePlayersTable =
                    $"{"Rank".PadLeft(rankPadding)}" +
                    $"{"Name".PadLeft(usernamePadding)}" +
                    $"{"Rating".PadLeft(eloRatingPadding)}" +
                    $"{"+/-".PadLeft(eloChangePadding)}" +
                    $"\n";

                foreach (var item in game.GamePlayers)
                {
                    var rank = item.Rank.ToString().PadLeft(rankPadding);
                    var username = item.Nickname.PadLeft(usernamePadding);
                    var eloRating = item.PostGameEloRating.ToString().PadLeft(eloRatingPadding);
                    var eloChange = item.EloChange.Value.ToString("+#;-#;0").PadLeft(eloChangePadding);

                    gamePlayersTable += $"{rank}{username}{eloRating}{eloChange}\n";
                }
            }

            //create embed
            var builder = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle($"Game {game.GameId} Summary")
                .AddField("Game Mode", (GameMode)game.Penalty, true)
                .AddField("Ranked", game.IsRanked ? "Yes" : "No", true)
                .AddField("Rounds", game.RoundCount, true)
                .AddField("Time", $"{game.DateStarted:yyyy-MM-dd HH:mm} PST ({game.DurationInMinutes} mins)")
                .AddField($"Players ({game.PlayerCount})", $"```{gamePlayersTable}```");

            if (game.Notes.Any())
            {
                var notes = game.Notes
                    .Select(n => $"**{GetUserNickname(n.Username, Context.Guild.Id)}**: {n.Text}")
                    .ToList();

                builder.AddField("Notes", string.Join("\n", notes));
            }

            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }

        private string GetUserNickname(string username, ulong guildId)
        {
            return _db.Players.AsQueryable()
                .Where(p => p.Username == username)
                .Where(p => p.GuildId == guildId)
                .FirstOrDefault()
                .Nickname;
        }
    }
}
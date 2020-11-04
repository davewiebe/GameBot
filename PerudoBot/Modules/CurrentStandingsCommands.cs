using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task DisplayCurrentStandings(Game game)
        {
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0);
            var totalDice = players.Sum(x => x.NumberOfDice);

            var playerList = string.Join("\n", players.Select(x => $"`{x.NumberOfDice}` {GetUserNickname(x.Username)}"));

            var quickmaths = $"Quick maths: {totalDice}/3 = `{totalDice / 3.0:F2}`";
            if (game.NextRoundIsPalifico) quickmaths = $"Quick maths: {totalDice}/6 = `{totalDice / 6.0:F2}`";
            if (players.Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                quickmaths = $"Quick maths:\n" +
                    $"1 = `{600 / 6.0:F2}%`\n" +
                    $"2 = `{500 / 6.0:F2}%`\n" +
                    $"3 = `{400 / 6.0:F2}%`\n" +
                    $"4 = `{300 / 6.0:F2}%`\n" +
                    $"5 = `{200 / 6.0:F2}%`\n" +
                    $"6 = `{100 / 6.0:F2}%`";
            }

            var builder = new EmbedBuilder()
                .WithTitle("Current standings")
                .AddField("Players", $"{playerList}\n\nTotal dice left: `{totalDice}`\n{quickmaths}", inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

        private async Task DisplayCurrentStandingsForBots(Game game)
        {
            var players = GetPlayers(game);
            if (!players.Any(x => x.IsBot)) return;

            players = players.Where(x => x.NumberOfDice > 0).ToList();
            var totalDice = players.Sum(x => x.NumberOfDice);

            var currentStandings = new
            {
                Players = players.Select(x => new { Username = GetUserNickname(x.Username), DiceCount = x.NumberOfDice }),
                TotalPlayers = players.Count(),
                TotalDice = totalDice
            };

            await SendMessageAsync($"Current standings for bots: ||{JsonConvert.SerializeObject(currentStandings)}||");
        }
    }
}
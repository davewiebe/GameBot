using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        private async Task DisplayCurrentStandings(Data.Game game)
        {
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0);
            var totalDice = players.Sum(x => x.NumberOfDice);

            var playerList = string.Join("\n", players.Select(x => $"`{x.NumberOfDice}` {GetUserNickname(x.Username)}"));

            var builder = new EmbedBuilder()
                .WithTitle("Current standings")
                .AddField("Users", $"{playerList}\n\nTotal dice left: `{totalDice}`\nQuick maths: {totalDice}/3 = `{totalDice / 3.0:F2}`", inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
        private async Task DisplayCurrentStandingsForBots(Data.Game game)
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

            await SendMessage($"Current standings for bots: ||{JsonConvert.SerializeObject(currentStandings)}||");
        }
    }
}

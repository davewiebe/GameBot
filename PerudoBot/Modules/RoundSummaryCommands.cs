using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PerudoBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task SendRoundSummaryForBots(Game game)
        {
            var players = GetGamePlayers(game);
            if (!players.Any(x => x.Player.IsBot)) return;

            var playerDice = players.Where(x => x.NumberOfDice > 0).ToList()
                .Select(x => new
                {
                    Username = x.Player.Nickname,
                    x.Dice
                });

            await SendMessageAsync($"Round summary for bots: ||{JsonConvert.SerializeObject(playerDice)}||");
        }

        private async Task SendRoundSummary(Game game)
        {
            var players = GetGamePlayers(game).Where(x => x.Dice != "").Where(x => x.NumberOfDice > 0).ToList();
            var playerDice = players.Select(x => $"{x.Player.Nickname}: {string.Join(" ", x.Dice.Split(",").Select(x => int.Parse(x).GetEmoji()))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            var allDiceGrouped = allDice
                .GroupBy(x => x)
                .OrderBy(x => x.Key);

            var countOfOnes = allDiceGrouped.SingleOrDefault(x => x.Key == 1)?.Count();

            var listOfAllDiceCounts = allDiceGrouped.Select(x => $"`{x.Count()}` ˣ {x.Key.GetEmoji()}");

            List<string> totals = new List<string>();
            for (int i = game.LowestPip; i <= game.HighestPip; i++)
            {
                var countOfX = allDiceGrouped.SingleOrDefault(x => x.Key == i)?.Count();
                var count1 = countOfOnes ?? 0;
                var countX = countOfX ?? 0;
                totals.Add($"`{count1 + countX }` ˣ {i.GetEmoji()}");
            }

            var builder = new EmbedBuilder()
                .WithTitle("Round Summary")
                .AddField("Players", $"{string.Join("\n", playerDice)}", inline: true)
                .AddField("Dice", $"{string.Join("\n", listOfAllDiceCounts)}", inline: true)
                .AddField("Totals", $"{string.Join("\n", totals)}", inline: true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}
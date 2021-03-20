using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task DisplayCurrentStandings(Game game)
        {
            var players = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0);
            var totalDice = players.Sum(x => x.NumberOfDice);







            var playerList = string.Join("\n", players.Select(x =>
                $"`{x.NumberOfDice}` {x.PlayerId.GetChristmasEmoji(game.Id)} {x.Player.Nickname} {GetGhostStatus(x)}{GetDealStatus(x)}"));


            var diceRange = game.HighestPip - game.LowestPip + 1;
            //var wildsEnabled = game.LowestPip == 1;
            var probability = diceRange * 1.0;
            probability = probability / 2.0;

            var probabilityString = $"{probability:F1}";
            if (probability == Math.Floor(probability))
            {
                probabilityString = $"{probability:F0}";
            }

            var quickmaths = $"Quick maths: {totalDice}/{probabilityString} = `{totalDice / probability:F2}`";
            //if (game.NextRoundIsPalifico) quickmaths = $"Quick maths: {totalDice}/{diceRange} = `{totalDice / (diceRange * 1.0):F2}`";
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
            //if (game.LowestPip != 1 || game.HighestPip != 6)  quickmaths = "Quickmaths: :upside_down:";

            var builder = new EmbedBuilder()
                .WithTitle(":deciduous_tree: Current standings :deciduous_tree:")
                .AddField("Players", $"{playerList}\n\nTotal dice left: `{totalDice}`\n{quickmaths}", inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

        private static string GetGhostStatus(Data.GamePlayer x)
        {
            return (x.GhostAttemptsLeft == -1 ? ":ghost:" : "");
        }
        private string GetDealStatus(Data.GamePlayer player)
        {
            if (player.UserDealIds == null) return "";
            var monkey = player.UserDealIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(y => int.Parse(y)).ToList();
            var monkey2 = player.PendingUserDealIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(y => int.Parse(y)).ToList();

            var returnme = "";
            foreach (var item in monkey)
            {
                var owedPlayer = _db.GamePlayers.SingleOrDefault(x => x.NumberOfDice > 0 && x.Id == item);
                if (owedPlayer != null) returnme += $":money_with_wings: {owedPlayer.Player.Nickname} ";
            }
            foreach (var item in monkey2)
            {
                var owedPlayer = _db.GamePlayers.SingleOrDefault(x => x.NumberOfDice > 0 && x.Id == item);
                if (owedPlayer != null) returnme += $":money_with_wings: {owedPlayer.Player.Nickname} ";
            }

            return returnme;
        }

        private async Task DisplayCurrentStandingsForBots(Game game)
        {
            return; // Andrey: Don't need bot standings for now, disabled to message spam
            var gamePlayers = _perudoGameService.GetGamePlayers(game);
            if (!gamePlayers.Any(x => x.Player.IsBot)) return;

            gamePlayers = gamePlayers.Where(x => x.NumberOfDice > 0).ToList();
            var totalDice = gamePlayers.Sum(x => x.NumberOfDice);

            var currentStandings = new
            {
                Players = gamePlayers.Select(x => new { Username = x.Player.Nickname, DiceCount = x.NumberOfDice }),
                TotalPlayers = gamePlayers.Count(),
                TotalDice = totalDice
            };

            await SendMessageAsync($"Current standings for bots: ||{JsonConvert.SerializeObject(currentStandings)}||");
        }
    }
}
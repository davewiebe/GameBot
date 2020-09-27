﻿using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("status")]
        public async Task Status()
        {
            var game = GetGame(GameState.Setup);
            if (game != null)
            {
                var players = GetPlayers(game);
                var options = GetOptions(game);
                var playersListString = string.Join("\n", players.Select(x => GetUserNickname(x.Username)));
                if (players.Count() == 0) playersListString = "none";

                var builder = new EmbedBuilder()
                                .WithTitle($"Game set up")
                                .AddField($"Players ({players.Count()})", $"{playersListString}", inline: false)
                                .AddField("Options", $"{string.Join("\n", options)}", inline: false);
                var embed = builder.Build();

                await Context.Channel.SendMessageAsync(
                    embed: embed)
                    .ConfigureAwait(false);
                return;
            }

            game = GetGame(GameState.InProgress);
            if (game != null)
            {
                var nextPlayer = GetCurrentPlayer(game);
                var bid = GetMostRecentBid(game);
                await DisplayCurrentStandings(game);

                var options = GetOptions(game);
                var builder = new EmbedBuilder()
                                .WithTitle("Game options")
                                .AddField("Options", $"{string.Join("\n", options)}", inline: false);
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                var recentBidText = "";
                if (bid != null)
                {
                    recentBidText = $"The most recent bid was for `{ bid.Quantity}` ˣ { bid.Pips.GetEmoji()}\n";
                }
                await SendMessage($"{recentBidText}It's {GetUserNickname(nextPlayer.Username)}'s turn.");
                return;
            }
            await SendMessage("There are no games in progress.");
        }
    }
}
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
            var game = await GetGameAsync(GameState.Setup);
            if (game != null)
            {
                var gamemode = "Variable";
                if (game.Penalty > game.NumberOfDice)
                {
                    gamemode = "SuddenDeath";
                }

                var players = _perudoGameService.GetGamePlayers(game).OrderByDescending(x => x.Player.EloRatings.FirstOrDefault(x => x.GameMode == gamemode)?.Rating);
                var options = _perudoGameService.GetOptions(game);
                var playersListString = string.Join("\n", players.Select(x => $"{x.PlayerId.GetChristmasEmoji(game.Id)} `{x.Player.EloRatings.FirstOrDefault(x => x.GameMode == gamemode)?.Rating}` {x.Player.Nickname}"));

                if (players.Count() == 0) playersListString = "none";

                var builder = new EmbedBuilder()
                                .WithTitle($"Game set up")
                                .AddField($"Players ({players.Count()})", $"{playersListString}", inline: false)
                                .AddField("Options", $"{string.Join("\n", options)}", inline: false);
                var embed = builder.Build();

                if (game.StatusMessage > 0)
                {
                    try
                    {
                        var lastMessage = await Context.Channel.GetMessageAsync(game.StatusMessage);
                        _ = lastMessage.DeleteAsync();
                    }
                    catch
                    { }
                }

                var monkey = await Context.Channel.SendMessageAsync(
                    embed: embed)
                    .ConfigureAwait(false);

                game.StatusMessage = monkey.Id;
                _db.SaveChanges();

                
                await monkey.AddReactionsAsync(new[] {
                    new Emoji("➕"),
                    //new Emoji("🔥"),
                    new Emoji("🪓"), // Axe

                   // new Emoji("🤥"),
                   // new Emoji("🙃"),➖
                    //new Emoji("🏅"),
                    new Emoji("➖"),

                }); //new Emoji("🤖") {🤥}{🔥}{🏅}
                return;
            }

            game = await GetGameAsync(GameState.InProgress);
            if (game != null)
            {
                var nextPlayer = GetCurrentPlayer(game);
                var bid = GetMostRecentBid(game);
                await DisplayCurrentStandings(game);

                var options = _perudoGameService.GetOptions(game);
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
                await SendMessageAsync($"{recentBidText}It's {nextPlayer.Player.Nickname}'s turn.");
                return;
            }
            await SendMessageAsync("There are no games in progress.");
        }
    }
}
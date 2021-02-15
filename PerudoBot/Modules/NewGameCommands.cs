using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewGameAsync(params string[] stringArray)
        {
            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .Where(x => x.State == (int)(object)GameState.InProgress
                        || x.State == (int)(object)GameState.Setup)
               .Any())
            {
                string message = $"A game already being set up or is in progress.";
                await SendMessageAsync(message);
                return;
            }

            var game = new Game
            {
                ChannelId = Context.Channel.Id,
                State = 0,
                DateCreated = DateTime.Now,
                NumberOfDice = 5,
                Penalty = 0,
                NextRoundIsPalifico = false,
                RandomizeBetweenRounds = false,
                WildsEnabled = true,
                ExactCallBonus = 1,
                ExactCallPenalty = 0,
                CanCallExactAnytime = true,
                CanCallLiarAnytime = true,
                CanBidAnytime = false,
                Palifico = true,
                IsRanked = true,
                GuildId = Context.Guild.Id,
                FaceoffEnabled = true,
                CanCallExactToJoinAgain = false,
                StatusMessage = 0,
                LowestPip = 1,
                HighestPip = 6,
                PenaltyGainDice = false,
                TerminatorMode = false
            };
            _db.Games.Add(game);
            _db.SaveChanges();

            var commands =
                $"`!add/remove @player` to add/remove players.\n" +
                $"`!option xyz` to set round options.\n" +
                $"`!status` to view current status.\n" +
                $"`!start` to start the game.";

            var builder = new EmbedBuilder()
                            .WithTitle($"New game #{game.Id} created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            AddUsers(game, Context.Message);
            try
            {
                await SetOptionsAsync(stringArray);
            }
            catch (Exception e)
            {
                var a = "monkey";
            }

            await Status();
            await ReplyAsync("PS: Special Snowflake (Palifico) rounds _now count wilds_");
            await ReplyAsync("PPS: Exact will get you a die back _even when it's your turn_");
        }
    }
}
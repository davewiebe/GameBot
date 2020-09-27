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
        public async Task NewGame(params string[] stringArray)
        {
            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .Where(x => x.State == (int)(object)GameState.InProgress
                        || x.State == (int)(object)GameState.Setup)
               .Any())
            {
                string message = $"A game already being set up or is in progress.";
                await SendMessage(message);
                return;
            }

            _db.Games.Add(new Game
            {
                ChannelId = Context.Channel.Id,
                State = 0,
                DateCreated = DateTime.Now,
                NumberOfDice = 5,
                Penalty = 1,
                NextRoundIsPalifico = false,
                RandomizeBetweenRounds = false,
                WildsEnabled = true,
                ExactCallBonus = 0,
                ExactCallPenalty = 0,
                CanCallExactAnytime = false,
                CanCallLiarAnytime = false,
                CanBidAnytime = false,
                Palifico = true,
                IsRanked = true,
                GuildId = Context.Guild.Id,
                FaceoffEnabled = true,
                CanCallExactToJoinAgain = false,
                StatusMessage = 0
            });
            _db.SaveChanges();

            var commands =
                $"`!add/remove @user` to add/remove players.\n" +
                $"`!option xyz` to set round options.\n" +
                $"`!status` to view current status.\n" +
                $"`!start` to start the game.";

            var builder = new EmbedBuilder()
                            .WithTitle("New game created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            var game = GetGame(GameState.Setup);
            AddUsers(game, Context.Message);
            try
            {
                SetOptions(stringArray);
            }
            catch (Exception e)
            {
                var a = "monkey";
            }

            await Status();
        }
    }
}
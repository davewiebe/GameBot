using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("same")]
        public async Task Same(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("anotherround")]
        public async Task AnotherRound(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("again")]
        public async Task Again(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("replay")]
        public async Task Replay(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("redo")]
        public async Task Redo(params string[] stringArray)
        {
            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .SingleOrDefault(x => x.State == IN_PROGRESS || x.State == SETUP) != null)
            {
                string message = $"A game is already in progress.";
                await SendMessage(message);
                return;
            }

            var lastGame = _db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .OrderByDescending(x => x.Id)
                .First();


            _db.Games.Add(new Data.Game
            {
                ChannelId = Context.Channel.Id,
                State = 0,
                DateCreated = DateTime.Now,
                NumberOfDice = lastGame.NumberOfDice,
                Penalty = lastGame.Penalty,
                NextRoundIsPalifico = false,
                RandomizeBetweenRounds = lastGame.RandomizeBetweenRounds,
                WildsEnabled = lastGame.WildsEnabled,
                ExactCallBonus = lastGame.ExactCallBonus,
                ExactCallPenalty = lastGame.ExactCallPenalty,
                CanCallExactAnytime = lastGame.CanCallExactAnytime,
                CanCallLiarAnytime = lastGame.CanCallLiarAnytime,
                CanBidAnytime = lastGame.CanBidAnytime,
                Palifico = lastGame.Palifico,
                IsRanked = lastGame.IsRanked,
                GuildId = Context.Guild.Id,
                FaceoffEnabled = lastGame.FaceoffEnabled
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


            var game = GetGame(SETUP);
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

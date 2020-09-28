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
        [Command("redo")]
        [Alias("same anotherround replay again playanother another")]
        public async Task Redo(params string[] stringArray)
        {
            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .Where(x => x.State == (int)(object)GameState.InProgress 
                    || x.State == (int)(object)GameState.Setup)
                .Any())
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

            _db.Games.Add(new Game
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
                FaceoffEnabled = lastGame.FaceoffEnabled,
                CanCallExactToJoinAgain = lastGame.CanCallExactToJoinAgain
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
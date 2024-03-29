﻿using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Extensions;
using Discord.WebSocket;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands// : ModuleBase<CommandContext>
    {
        //[Command("➕")]
        public async Task AddPlayer(CommandContext context, ulong userid)
        {
            var game = await _perudoGameService.GetGameAsync(context.Channel.Id, GameState.Setup);
            //if (context.User.IsBot) return;

            var user = await context.Guild.GetUserAsync(userid);

            _perudoGameService.AddUserToGame(game, (SocketGuildUser)user);

            await UpdateStatus(context, game);
        }
        public async Task RemovePlayer(CommandContext context, ulong userid)
        {
            var game = await _perudoGameService.GetGameAsync(context.Channel.Id, GameState.Setup);
            //if (context.User.IsBot) return;

            var user = await context.Guild.GetUserAsync(userid);

            var userToRemove = _db.GamePlayers.FirstOrDefault(x => x.GameId == game.Id && x.Player.UserId == userid);
            if (userToRemove == null) return;

            _db.GamePlayers.Remove(userToRemove);
            _db.SaveChanges();

            await UpdateStatus(context, game);
        }

        [Command("🔥")]
        [Alias("🪓")] // axe
        public async Task DiceOption(CommandContext context)
        {
            var game = await _perudoGameService.GetGameAsync(context.Channel.Id, GameState.Setup);
            if (context.Message.Id != game.StatusMessage) return;

            if (game.Penalty == 0)
            {
                game.Penalty = 100;
            }
            else if (game.Penalty == 100)
            {
                game.Penalty = 0;
            }
            _db.SaveChanges();
            await UpdateStatus(context, game);
        }
        /*
        [Command("🤥")]
        public async Task LiarAnytimeOption()
        {
            var game = await GetGameAsync(GameState.Setup);
            if (Context.Message.Id != game.StatusMessage) return;

            game.CanCallLiarAnytime = !game.CanCallLiarAnytime;
            _db.SaveChanges();
            await UpdateStatus(game);
        }
        [Command("🏅")]
        public async Task RankedOption()
        {
            var game = await GetGameAsync(GameState.Setup);
            if (Context.Message.Id != game.StatusMessage) return;

            game.IsRanked = !game.IsRanked;
            _db.SaveChanges();
            await UpdateStatus(game);
        }

        [Command("🤖")]
        public async Task RobotOption()
        {
            var game = await GetGameAsync(GameState.Setup);
            if (Context.Message.Id != game.StatusMessage) return;

            game.TerminatorMode = !game.TerminatorMode;
            _db.SaveChanges();
            await UpdateStatus(game);
        }

        private async Task<Game> GetGameAsync(params GameState[] gameStates)
        {
            return await _perudoGameService.GetGameAsync(Context.Channel.Id, gameStates);
        }

        [Command("🙃")]
        public async Task ReverseOption()
        {
            var game = await GetGameAsync(GameState.Setup);
            if (Context.Message.Id != game.StatusMessage) return;

            game.PenaltyGainDice = !game.PenaltyGainDice;
            _db.SaveChanges();
            await UpdateStatus(game);
        }

        */
        private async Task UpdateStatus(CommandContext context, Game game)
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


            await context.Message.ModifyAsync(x => x.Embed = embed);

            //var monkey = await Context.Channel.SendMessageAsync(
            //    embed: embed)
            //    .ConfigureAwait(false);

            //game.StatusMessage = monkey.Id;
            //_db.SaveChanges();
        }
        
    }
}
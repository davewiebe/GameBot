﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task RollDiceStartNewRound(Game game)
        {
            _db.SaveChanges();
            // IF THERE IS ONLY ONE PLAYER LEFT, ANNOUNCE THAT THEY WIN
            var players = GetPlayers(game);

            var activePlayers = players.Where(x => x.NumberOfDice > 0);
            bool onlyOnePlayerLeft = activePlayers.Count() == 1;
            if (onlyOnePlayerLeft)
            {
                await SendMessage($":trophy: {GetUser(activePlayers.Single().Username).Mention} is the winner with `{activePlayers.Single().NumberOfDice}` dice remaining! :trophy:");

                var rattles = _db.Rattles.SingleOrDefault(x => x.Username == activePlayers.Single().Username);
                if (rattles != null)
                {
                    await SendMessage(rattles.Winrattle);
                }

                game.State = (int)GameState.Finished;
                game.DateFinished = DateTime.Now;
                game.Winner = activePlayers.Single().Username;
                _db.SaveChanges();
                return;
            }

            var r = new Random();

            var players2 = _db.Players
                .AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.NumberOfDice > 0)
                .ToList();

            var botKeys = _db.BotKeys.AsQueryable()
                .ToList();

            foreach (var player in players2)
            {
                List<int> dice = new List<int>();
                for (int i = 0; i < player.NumberOfDice; i++)
                {
                    dice.Add(r.Next(1, 7));
                }

                dice.Sort();

                player.Dice = string.Join(",", dice);

                var user = Context.Guild.Users.Single(x => x.Username == player.Username);
                var message = $"Your dice: {string.Join(" ", dice.Select(x => x.GetEmoji()))}";

                var botKey = botKeys.FirstOrDefault(x => x.Username == player.Username);

                if (botKey == null)
                {
                    try
                    {
                        var requestOptions = new RequestOptions()
                        { RetryMode = RetryMode.RetryRatelimit };
                        await user.SendMessageAsync(message, options: requestOptions);
                    }
                    catch (Exception e)
                    {
                        await SendEncryptedDice(player, user, player.Username);
                    }
                }
                else
                {
                    await SendEncryptedDice(player, user, botKey.BotAesKey);
                }
            }
            _db.SaveChanges();

            if (game.RandomizeBetweenRounds) ShufflePlayers(game);

            await DisplayCurrentStandingsForBots(game);
            await DisplayCurrentStandings(game);

            _db.SaveChanges();
            game.RoundStartPlayerId = GetCurrentPlayer(game).Id;
            _db.SaveChanges();

            if (activePlayers.Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                await SendTempMessage("!gif fight");
                await SendMessage($":face_with_monocle: Faceoff Round :face_with_monocle: {GetUser(GetCurrentPlayer(game).Username).Mention} goes first. Bid on total pips only (eg. `!bid 4`)");
            }
            else if (game.NextRoundIsPalifico)
            {
                await SendMessage($":game_die: Palifico Round :game_die: {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.\n" +
                    $"`!exact` will only reset the round - no bonuses.");
            }
            else
            {
                await SendMessage($"A new round has begun. {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.");
            }
        }

        private async Task SendEncryptedDice(Player player, SocketGuildUser user, string botKey)
        {
            var diceText = $"{player.Dice.Replace(",", " ")}";
            var encoded = SimpleAES.AES256.Encrypt(diceText, botKey);
            await SendMessage($"{user.Mention}'s dice: ||{encoded}||");
        }
    }
}
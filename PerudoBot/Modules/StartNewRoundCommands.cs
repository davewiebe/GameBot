using Discord;
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
        private async Task RollDiceStartNewRoundAsync(Game game)
        {
            // mark the end of the current round
            if (game.CurrentRound != null)
            {
                game.CurrentRound.EndRound();
            }
            _db.SaveChanges();
            // IF THERE IS ONLY ONE PLAYER LEFT, ANNOUNCE THAT THEY WIN
            var gamePlayers = _perudoGameService.GetGamePlayers(game);

            var activeGamePlayers = gamePlayers.Where(x => x.NumberOfDice > 0);
            bool onlyOnePlayerLeft = activeGamePlayers.Count() == 1;
            if (onlyOnePlayerLeft)
            {
                await SendMessageAsync($":trophy: {GetUser(activeGamePlayers.Single().Player.Username).Mention} is the winner with `{activeGamePlayers.Single().NumberOfDice}` dice remaining! :trophy:");

                var rattles = _db.Rattles.SingleOrDefault(x => x.Username == activeGamePlayers.Single().Player.Username);
                if (rattles != null)
                {
                    await SendMessageAsync(rattles.Winrattle);
                }

                game.State = (int)GameState.Finished;
                game.DateFinished = DateTime.Now;
                game.Winner = activeGamePlayers.Single().Player.Username;
                _db.SaveChanges();
                return;
            }

            var r = new Random();

            var botKeys = _db.BotKeys.AsQueryable()
                .ToList();

            if (game.RandomizeBetweenRounds) ShufflePlayers(game);

            await DisplayCurrentStandingsForBots(game);
            await DisplayCurrentStandings(game);

            _db.SaveChanges();
            game.RoundStartPlayerId = GetCurrentPlayer(game).Id;
            _db.SaveChanges();

            Round round;

            if (activeGamePlayers.Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                round = new FaceoffRound()
                {
                    GameId = game.Id,
                    RoundNumber = (game.CurrentRound?.RoundNumber ?? 0) + 1,
                    StartingPlayerId = GetCurrentPlayer(game).Id
                };

                await SendTempMessageAsync("!gif snowball fight");
                await SendMessageAsync($":face_with_monocle: Faceoff Round :face_with_monocle: {GetUser(GetCurrentPlayer(game).Player.Username).Mention} goes first. Bid on total pips only (eg. `!bid 4`)");
            }
            else if (game.NextRoundIsPalifico)
            {
                round = new PalificoRound()
                {
                    GameId = game.Id,
                    RoundNumber = (game.CurrentRound?.RoundNumber ?? 0) + 1,
                    StartingPlayerId = GetCurrentPlayer(game).Id
                };

                await SendMessageAsync($":snowflake: Special Snowflake Round :snowflake: {GetUser(GetCurrentPlayer(game).Player.Username).Mention} goes first.\n" +
                    $"`!exact` will only reset the round - no bonuses.");
            }
            else
            {
                round = new StandardRound()
                {
                    GameId = game.Id,
                    RoundNumber = (game.CurrentRound?.RoundNumber ?? 0) + 1,
                    StartingPlayerId = GetCurrentPlayer(game).Id
                };
                await SendMessageAsync($"A new round has begun. {GetUser(GetCurrentPlayer(game).Player.Username).Mention} goes first.");
            }
            _db.Rounds.Add(round);

            foreach (var gamePlayer in activeGamePlayers)
            {
                var deals = new List<int>();
                if (!string.IsNullOrWhiteSpace(gamePlayer.UserDealIds))
                {
                    var currentDeals = gamePlayer.UserDealIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList()
                        .Select(x => int.Parse(x));

                    deals.AddRange(currentDeals);
                }

                if (!string.IsNullOrWhiteSpace(gamePlayer.PendingUserDealIds))
                {
                    var pendingDeals = gamePlayer.PendingUserDealIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList()
                        .Select(x => int.Parse(x));

                    deals.AddRange(pendingDeals);
                }
                gamePlayer.UserDealIds = string.Join(',', deals);
                gamePlayer.PendingUserDealIds = "";

                var dice = new List<int>();
                var numberOfDice = gamePlayer.NumberOfDice;

                //if (game.CurrentRoundNumber == 1 && game.PenaltyGainDice) numberOfDice = 1;

                for (int i = 0; i < numberOfDice; i++)
                {
                    dice.Add(r.Next(game.LowestPip, game.HighestPip + 1));
                }

                dice.Sort();

                var gamePlayerRound = new GamePlayerRound()
                {
                    GamePlayer = gamePlayer,
                    Round = round,
                    Dice = string.Join(",", dice),
                    IsGhost = gamePlayer.GhostAttemptsLeft == -1,
                    NumberOfDice = gamePlayer.NumberOfDice,
                    TurnOrder = -1 // Figure out out to assign turnorder based off starting
                };
                gamePlayer.Dice = string.Join(",", dice);
                _db.Add(gamePlayerRound);

                var user = Context.Guild.Users.Single(x => x.Username == gamePlayer.Player.Username);
                var message = $"Your dice: {string.Join(" ", dice.Select(x => x.GetEmoji()))}";

                var botKey = botKeys.FirstOrDefault(x => x.Username == gamePlayer.Player.Username);

                if (botKey == null)
                {
                    // TODO: use IsBot property to determine if DM can be sent
                    try
                    {
                        var requestOptions = new RequestOptions()
                        { RetryMode = RetryMode.RetryRatelimit };
                        await user.SendMessageAsync(message, options: requestOptions);
                    }
                    catch (Exception e)
                    {
                        await SendEncryptedDiceAsync(gamePlayer, user, gamePlayer.Player.Username);
                    }
                }
                else
                {
                    await SendEncryptedDiceAsync(gamePlayer, user, botKey.BotAesKey);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task SendEncryptedDiceAsync(GamePlayer player, SocketGuildUser user, string botKey)
        {
            var diceText = $"{player.Dice.Replace(",", " ")}";
            var encoded = SimpleAES.AES256.Encrypt(diceText, botKey);
            await SendMessageAsync($"{user.Mention}'s dice: ||{encoded}||");
        }
    }
}
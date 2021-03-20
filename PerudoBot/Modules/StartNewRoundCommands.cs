using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;
using Newtonsoft.Json;

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
                game.EndGame();

                // todo: I'd move this into game.EndGame() but honestly I'm scared it will break
                game.Winner = activeGamePlayers.Single().Player.Username;
                _db.SaveChanges();

                await _perudoGameService.UpdateGamePlayerRanksAsync(game.Id);
                await new EloRatingService(_db).GenerateEloRatingsForGameAsync(game.Id);
                await GetGameSummaryAsync(game.Id);
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

            if (activeGamePlayers.Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled && !AreBotsInGame(game))
            {
                round = new FaceoffRound()
                {
                    GameId = game.Id,
                    RoundNumber = (game.CurrentRound?.RoundNumber ?? 0) + 1,
                    StartingPlayerId = GetCurrentPlayer(game).Id
                };

                await SendTempMessageAsync("!gif lumberjack");
                await SendMessageAsync($":carpentry_saw: Faceoff Round :carpentry_saw: {GetUser(GetCurrentPlayer(game).Player.Username).Mention} goes first. Bid on total pips only (eg. `!bid 4`)");
            }
            else if (game.NextRoundIsPalifico && !AreBotsInGame(game))
            {
                round = new PalificoRound()
                {
                    GameId = game.Id,
                    RoundNumber = (game.CurrentRound?.RoundNumber ?? 0) + 1,
                    StartingPlayerId = GetCurrentPlayer(game).Id
                };

                await SendMessageAsync($":four_leaf_clover: Palifico Round :four_leaf_clover: {GetUser(GetCurrentPlayer(game).Player.Username).Mention} goes first.\n" +
                    $"`wilds count now` `only players at 1 die can change the pips`");
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

                if (AreBotsInGame(game))
                {
                    var botMessage = new
                    {
                        nextPlayer = GetUser(GetCurrentPlayer(game).Player.Username).Id.ToString(),
                        diceCount = _perudoGameService.GetGamePlayers(game).Sum(x => x.NumberOfDice),
                        round = round.RoundNumber
                    };

                    await SendMessageAsync($"||`@bots update {JsonConvert.SerializeObject(botMessage)}`||");
                }
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

                if (user.IsBot) {
                    var botKeyString = (botKey != null) ? botKey.BotAesKey : gamePlayer.Player.Username;
                    await SendEncryptedDiceAsync(gamePlayer, user, botKeyString);
                }
                else
                {
                    var requestOptions = new RequestOptions()
                    { RetryMode = RetryMode.RetryRatelimit };
                    await user.SendMessageAsync(message, options: requestOptions);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task SendEncryptedDiceAsync(GamePlayer player, SocketGuildUser user, string botKey)
        {
            var diceText = $"{player.Dice.Replace(",", " ")}";
            var encoded = SimpleAES.AES256.Encrypt(diceText, botKey);
            await SendMessageAsync($"{user.Mention} ||`deal {encoded}`||");
        }
    }
}
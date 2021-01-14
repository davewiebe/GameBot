using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private SocketGuildUser GetUser(string username)
        {
            return Context.Guild.Users.FirstOrDefault(x => x.Username == username);
        }

        private string GetUserNickname(string username)
        {
            var user = GetUser(username);
            if (user == null) return username;

            var nickname = user.Nickname;
            if (string.IsNullOrEmpty(nickname)) return username;
            if (nickname.StripSpecialCharacters().Trim() == "") return "NULL";
            return nickname.StripSpecialCharacters();
        }

        private void SetTurnPlayerToRoundStartPlayer(Game game)
        {
            game.PlayerTurnId = game.RoundStartPlayerId;

            var thatUser = _perudoGameService.GetGamePlayers(game).Single(x => x.Id == game.PlayerTurnId);
            if (thatUser.NumberOfDice == 0)
            {
                SetNextPlayer(game, thatUser);
            }

            _db.SaveChanges();
        }

        private bool AreBotsInGame(Game game)
        {
            var gamePlayers = _perudoGameService.GetGamePlayers(game);
            return gamePlayers.Any(x => x.Player.IsBot);
        }

        public bool PlayerEligibleForSafeguard(Game game, int numberOfDice, int penalty)
        {
            if (game.PenaltyGainDice && game.Penalty == 0 && numberOfDice < 5 && penalty + numberOfDice > 5) return true;
            if (game.PenaltyGainDice) return false;

            // eligible if variable mode and player is about to lose all his dice without getting down to 1
            if (game.Penalty == 0 && numberOfDice > 1 && penalty >= numberOfDice)
            {
                return true;
            }

            return false;
        }

        private async Task DecrementDieFromPlayer(GamePlayer player, int penalty)
        {
            player.NumberOfDice -= penalty;
            player.CurrentGamePlayerRound.Penalty = penalty;

            var game = await GetGameAsync(GameState.InProgress);
            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice <= 0)
            {
                player.CurrentGamePlayerRound.IsEliminated = true;

                await SendMessageAsync($":candle::droplet::candle: {player.Player.Nickname} melted :candle::droplet::candle:");
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Player.Username);
                if (deathrattle != null)
                {
                    await SendMessageAsync(deathrattle.Deathrattle);
                }

                if (game.CanCallExactToJoinAgain)
                {
                    if (_perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Count() > 2)
                    {
                        if (player.GhostAttemptsLeft != -1)
                        {
                            player.GhostAttemptsLeft = 3;
                            _db.SaveChanges();
                            await SendMessageAsync($":hourglass::hourglass: {GetUserNickname(player.Player.Username)} you have `3` attempts at an `!exact` call to win your way back into the game (3+ players).");
                        }
                    }
                }
            }

            if (player.NumberOfDice == 1 && game.Palifico)
            {
                game.NextRoundIsPalifico = true;
            }
            else
            {
                game.NextRoundIsPalifico = false;
            }
            _db.SaveChanges();

            var gameService = new PerudoGameService(_db);
            await gameService.UpdateGamePlayerRanksAsync(game.Id);
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Game game, GamePlayer player, int penalty)
        {
            if (game.PenaltyGainDice)
            {
                player.NumberOfDice += penalty;
                if (player.NumberOfDice > game.NumberOfDice) player.NumberOfDice = 0;
            }
            else
            {
                player.NumberOfDice -= penalty;
            }
            
            player.CurrentGamePlayerRound.Penalty = penalty;
            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice == 1 && game.Palifico)
            {
                game.NextRoundIsPalifico = true;
            }
            else
            {
                game.NextRoundIsPalifico = false;
            }

            if (player.NumberOfDice <= 0)
            {
                player.CurrentGamePlayerRound.IsEliminated = true;

                await SendMessageAsync($":candle::droplet::candle: {player.Player.Nickname} melted :candle::droplet::candle:");
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Player.Username);
                if (deathrattle != null)
                {
                    await SendMessageAsync(deathrattle.Deathrattle);
                }

                if (game.CanCallExactToJoinAgain)
                {
                    if (_perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Count() > 2)
                    {
                        if (player.GhostAttemptsLeft != -1)
                        {
                            player.GhostAttemptsLeft = 3;
                            _db.SaveChanges();
                            await SendMessageAsync($":hourglass::hourglass: {player.Player.Nickname} you have `3` attempts at an `!exact` call to win your way back into the game (3+ players).");
                        }
                    }
                }

                SetNextPlayer(game, player);
            }
            else
            {
                game.PlayerTurnId = player.Id;
                _db.SaveChanges();
            }
        }

        private int GetNumberOfDiceMatchingBid(Game game, int pips)
        {
            var players = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).ToList();

            if (game.FaceoffEnabled && players.Sum(x => x.NumberOfDice) == 2)
            {
                var allDice2 = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
                return allDice2.Sum();
            }

            var allDice = players.Where(x => x.Dice != "").SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));

            if (game.NextRoundIsPalifico)
            {
                return allDice.Count(x => x == pips);
            }
            return allDice.Count(x => x == pips || x == 1);
        }



        private GamePlayer GetCurrentPlayer(Game game)
        {
            var playerTurnId = game.PlayerTurnId;

            return _db.GamePlayers
                .Include(gp => gp.Player)
                .Include(gp => gp.GamePlayerRounds)
                .AsQueryable()
                .Single(x => x.Id == playerTurnId);
        }

        private GamePlayer GetActivePlayer(Game game)
        {
            var playerTurnId = game.PlayerTurnId;
            if (game.DealCurrentGamePlayerId != 0) playerTurnId = game.DealCurrentGamePlayerId;

            return _db.GamePlayers
                .Include(gp => gp.Player)
                .Include(gp => gp.GamePlayerRounds)
                .AsQueryable()
                .Single(x => x.Id == playerTurnId);
        }

        private void RemoveActiveDeals(Game game)
        {
            game.GamePlayers.All(x => x.HasActiveDeal = false);
            _db.SaveChanges();
        }

        private void RemovePayupPlayer(Game game)
        {
            game.DealCurrentGamePlayerId = 0;
            _db.SaveChanges();
        }


        private void SetNextPlayer(Game game, GamePlayer currentPlayer)
        {
            var playerIds = _db.GamePlayers
                .AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.NumberOfDice > 0 || x.Player.Username == currentPlayer.Player.Username) // in case the current user is eliminated and won't show up
                .OrderBy(x => x.TurnOrder)
                .Select(x => x.Id)
                .ToList();

            var playerIndex = playerIds.FindIndex(x => x == currentPlayer.Id);

            if (playerIndex >= playerIds.Count - 1)
            {
                game.PlayerTurnId = playerIds.ElementAt(0);
            }
            else
            {
                game.PlayerTurnId = playerIds.ElementAt(playerIndex + 1);
            }

            _db.SaveChanges();
        }
    }
}
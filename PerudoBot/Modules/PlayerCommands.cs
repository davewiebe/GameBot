using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private List<Player> GetPlayers(Game game)
        {
            return _db.Players.AsQueryable()
                .Where(x => x.GameId == game.Id)
                .OrderBy(x => x.TurnOrder)
                .ToList();
        }

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

            var thatUser = GetPlayers(game).Single(x => x.Id == game.PlayerTurnId);
            if (thatUser.NumberOfDice == 0)
            {
                SetNextPlayer(game, thatUser);
            }

            _db.SaveChanges();
        }

        private bool AreBotsInGame(Game game)
        {
            var players = GetPlayers(game);
            return players.Any(x => x.IsBot);
        }

        public bool PlayerEligeebleForSafeguard(bool isVariable, int numberOfDice, int penalty)
        {
            // eligeeble if variable mode and player is about to lose all his dice without getting down to 1
            if (isVariable && numberOfDice > 1 && penalty >= numberOfDice)
            {
                return true;
            }

            return false;
        }

        private async Task DecrementDieFromPlayer(Player player, int penalty)
        {
            player.NumberOfDice -= penalty;

            var game = await GetGameAsync(GameState.InProgress);
            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice <= 0)
            {
                await SendMessageAsync($":fire::skull::fire: {GetUserNickname(player.Username)} defeated :fire::skull::fire:");
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Username);
                if (deathrattle != null)
                {
                    await SendMessageAsync(deathrattle.Deathrattle);
                }

                if (game.CanCallExactToJoinAgain)
                {
                    if (GetPlayers(game).Where(x => x.NumberOfDice > 0).Count() > 2)
                    {
                        if (player.GhostAttemptsLeft != -1)
                        {
                            player.GhostAttemptsLeft = 3;
                            _db.SaveChanges();
                            await SendMessageAsync($":hourglass::hourglass: {GetUserNickname(player.Username)} you have `3` attempts at an `!exact` call to win your way back into the game (3+ players).");
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
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Game game, Player player, int penalty)
        {
            player.NumberOfDice -= penalty;

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
                await SendMessageAsync($":fire::skull::fire: {GetUserNickname(player.Username)} defeated :fire::skull::fire:");
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Username);
                if (deathrattle != null)
                {
                    await SendMessageAsync(deathrattle.Deathrattle);
                }

                if (game.CanCallExactToJoinAgain)
                {
                    if (GetPlayers(game).Where(x => x.NumberOfDice > 0).Count() > 2)
                    {
                        if (player.GhostAttemptsLeft != -1)
                        {
                            player.GhostAttemptsLeft = 3;
                            _db.SaveChanges();
                            await SendMessageAsync($":hourglass::hourglass: {GetUserNickname(player.Username)} you have `3` attempts at an `!exact` call to win your way back into the game (3+ players).");
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
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0).ToList();

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

        private Player GetCurrentPlayer(Game game)
        {
            return _db.Players
                .AsQueryable()
                .Single(x => x.Id == game.PlayerTurnId);
        }

        private void SetNextPlayer(Game game, Player currentPlayer)
        {
            var playerIds = _db.Players
                .AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.NumberOfDice > 0 || x.Username == currentPlayer.Username) // in case the current user is eliminated and won't show up
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
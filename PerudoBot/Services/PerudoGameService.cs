using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class PerudoGameService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public PerudoGameService(GameBotDbContext db)
        {
            _db = db;
        }

        public async Task TerminateGameAsync(int gameId)
        {
            var gameToTerminate = await _db.Games.AsQueryable()
                .SingleAsync(g => g.Id == gameId);

            if (gameToTerminate.State != (int)GameState.Finished)
            {
                gameToTerminate.State = (int)GameState.Terminated;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Game> GetGameAsync(ulong channelId, params GameState[] gameStates)
        {
            var gameStateIds = gameStates.Cast<int>().ToList();

            return await _db.Games.AsQueryable()
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.Actions)
                .Where(x => x.ChannelId == channelId)
                .Where(x => gameStateIds.Contains(x.State))
                .SingleOrDefaultAsync();
        }

        public Player GetCurrentPlayer(Game game)
        {
            return _db.Players
                .AsQueryable()
                .Single(x => x.Id == game.PlayerTurnId);
        }

        public void SetNextPlayer(Game game, Player currentPlayer)
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

        public async Task<LiarCallResult> HandleLiarCallAsync(LiarCallRequest liarCallRequest)
        {
            if (liarCallRequest.PreviousBid == null) throw new NullReferenceException(
                 nameof(liarCallRequest.PreviousBid));

            var result = new LiarCallResult();
            // check if game is in progress

            var game = liarCallRequest.Game;
            var biddingPlayer = liarCallRequest.Caller;

            //get the player who's turn it is right now
            var playerWhoShouldGoNext = GetCurrentPlayer(game);

            // create liar call object
            var liarCall = new LiarCall()
            {
                PlayerId = playerWhoShouldGoNext.Id,
                Round = game.CurrentRound,
                ParentAction = liarCallRequest.PreviousBid,
                IsSuccess = true,
                IsOutOfTurn = false
            };

            if (game.CanCallLiarAnytime)
            {
                // get the player making the liar call with at least on die,
                var player = _db.Players.AsQueryable()
                    .Where(x => x.GameId == game.Id)
                    .Where(x => x.NumberOfDice > 0)
                    .Where(x => x.Username == liarCallRequest.Caller.Username)
                    .SingleOrDefault();

                // if non found (not in game) exit
                if (player == null)
                {
                    result.ErrorMessage = "Player not found in game.";
                }

                // check if calling player is calling out of turn
                if (game.PlayerTurnId != player.Id)
                {
                    //player is calling out of turn

                    // change the game's current player to the out of turn player
                    game.PlayerTurnId = player.Id;

                    // save changes so game is updated
                    _db.SaveChanges();

                    // biddingPlayer
                    playerWhoShouldGoNext = GetCurrentPlayer(game);
                    liarCall.IsOutOfTurn = true;
                }
            }
            else
            {
                // make sure player calling liar is the player who should go next
                if (playerWhoShouldGoNext.Username != liarCallRequest.Caller.Username)
                {
                    result.ErrorMessage = "Can't call liar out of turn";
                }
            }

            //DeleteCommandFromDiscord();

            //// send message that liar has been called, w/ details
            //await SendMessageAsync($"{GetUserNickname(playerWhoShouldGoNext.Username)} called **liar** on `{previousBid.Quantity}` ˣ {biddingObject}.");

            //// for the dramatic affect
            //Thread.Sleep(4000);

            // GetNumberOfDiceMatchingBid
            int numberOfDiceMatchingBid = GetNumberOfDiceMatchingBid(game, liarCallRequest.PreviousBid.Pips);
            result.ActualQuantity = numberOfDiceMatchingBid;

            if (numberOfDiceMatchingBid >= liarCallRequest.PreviousBid.Quantity)
            {
                // there are more matching dice than previous bid (previous bid was good)
                liarCall.IsSuccess = false;
                result.IsSuccess = false;

                //determine penalty
                var penalty = (numberOfDiceMatchingBid - liarCallRequest.PreviousBid.Quantity) + 1; // if variable penalty
                if (game.Penalty != 0) penalty = game.Penalty; // penalty is set to 0 for variable penalty games
                result.Penalty = penalty;

                // send outcome of unsuccessful liar call
                //await SendMessageAsync($"There was actually `{numberOfDiceMatchingBid}` {biddingName}. :fire: {GetUser(playerWhoShouldGoNext.Username).Mention} loses {penalty} dice. :fire:");

                // if matching dice is exactly what previous bid was, send that taunt!
            }
            else
            {
                liarCall.IsSuccess = true;
                result.IsSuccess = true;

                // do same with penalty as in the if statement
                var penalty = liarCallRequest.PreviousBid.Quantity - numberOfDiceMatchingBid;
                if (game.Penalty != 0) penalty = game.Penalty;
                result.Penalty = penalty;
            }

            _db.Actions.Add(liarCall);
            _db.SaveChanges();

            return result;
        }

        public List<Player> GetPlayers(Game game)
        {
            return _db.Players.AsQueryable()
                .Where(x => x.GameId == game.Id)
                .OrderBy(x => x.TurnOrder)
                .ToList();
        }

        private int GetNumberOfDiceMatchingBid(Game game, int pips)
        {
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0).ToList();

            if (game.FaceoffEnabled && players.Sum(x => x.NumberOfDice) == 2)
            {
                var allDice2 = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
                return allDice2.Sum();
            }

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));

            if (game.NextRoundIsPalifico)
            {
                return allDice.Count(x => x == pips);
            }
            return allDice.Count(x => x == pips || x == 1);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
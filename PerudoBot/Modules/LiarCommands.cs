﻿using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("liar")]
        [Alias("naughty", "Iiar", "li", "lair", "la", "l!ar", "Iair", "llar", "l", "lia", "lai", "llar")]
        public async Task LiarAsync(params string[] stringArray)
        {
            // check if game is in progress
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            // get inprogress game
            var game = await GetGameAsync(GameState.InProgress);

            // get latest bid and make sure not null or 0 (because liar or exact from last round
            //will have quant&pips of 0&0
            var previousBid = game.CurrentRound.LatestAction as Bid;

            var lastAction = game.CurrentRound.LatestAction;
            //_db.Actions
            //    .AsQueryable()
            //    .Where(g => g.GameId == game.Id)
            //    .LastOrDefault();

            if (lastAction is Bid == false || lastAction == null)
                return;
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;

            //get the player who's turn it is right now
            var playerWhoseTurnItIs = GetCurrentPlayer(game);
            var originalPlayerWhoseTurnItIs = GetCurrentPlayer(game);

            var activePlayer = GetActivePlayer(game);


            // create liar call object
            var liarCall = new LiarCall()
            {
                GamePlayer = playerWhoseTurnItIs,
                Round = game.CurrentRound,
                GamePlayerRound = playerWhoseTurnItIs.CurrentGamePlayerRound,
                ParentAction = previousBid,
                IsSuccess = true,
                IsOutOfTurn = false
            };

            if (playerWhoseTurnItIs.CurrentGamePlayerRound.IsAutoLiarSet)
            {
                liarCall.IsAutoAction = true;
            }
            else if (game.CanCallLiarAnytime)
            {
                // get the player making the liar call with at least on die,
                var gamePlayer = _db.GamePlayers.AsQueryable()
                    .Where(x => x.GameId == game.Id)
                    .Where(x => x.NumberOfDice > 0)
                    .Where(x => x.Player.Username == Context.User.Username)
                    .SingleOrDefault();

                // if none found (not in game) exit
                if (gamePlayer == null) return;

                // check if calling player is calling out of turn
                if (game.PlayerTurnId != gamePlayer.Id)
                {
                    //player is calling out of turn

                    // change the game's current player to the out of turn player
                    game.PlayerTurnId = gamePlayer.Id;
                    liarCall.GamePlayer = gamePlayer;

                    // save changes so game is updated
                    _db.SaveChanges();

                    // biddingPlayer
                    playerWhoseTurnItIs = GetCurrentPlayer(game);
                    liarCall.IsOutOfTurn = true;
                }
            }
            else // not liar anytime
            {
                // make sure player calling liar is the player who should go next
                if (playerWhoseTurnItIs.Player.Username != Context.User.Username)
                {
                    if(!activePlayer.HasActiveDeal) return;
                }
            }

            var playerWhoCalledLiar = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Single(x => x.Player.Username == Context.User.Username);

            if (activePlayer.HasActiveDeal && Context.User.Id != activePlayer.Player.UserId)
            {
                playerWhoCalledLiar.PendingUserDealIds = $"{playerWhoCalledLiar.PendingUserDealIds},{activePlayer.Id}";

                await SendMessageAsync($":money_mouth: {playerWhoCalledLiar.Player.Nickname} has accepted the deal! {playerWhoCalledLiar.Player.Nickname}, on your turn use `!payup @{activePlayer.Player.Nickname}` to force them to take your turn for you.");            }

            // set bidding style to dice (for everything but faceoff round)
            var biddingObject = previousBid.Pips.GetEmoji();
            var biddingName = "dice";

            // if its faceoff round, and it's enabled, use pips instead
            if (_perudoGameService.GetGamePlayers(game).Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                biddingObject = ":record_button:";
                biddingName = "pips";
            }

            DeleteCommandFromDiscord();
            // send message that liar has been called, w/ details
            var dealer = "";
            if (originalPlayerWhoseTurnItIs.Id != playerWhoCalledLiar.Id &&
                (originalPlayerWhoseTurnItIs.HasActiveDeal || game.DealCurrentGamePlayerId == playerWhoCalledLiar.Id))
            {
                dealer = $" (calling for {originalPlayerWhoseTurnItIs.Player.Nickname})";
            }
            await SendMessageAsync($"{playerWhoCalledLiar.Player.Nickname}{dealer} called **liar**{(liarCall.IsOutOfTurn ? " (out of turn)" : "")} on `{previousBid.Quantity}` ˣ {biddingObject}.");

            // for the dramatic affect
            Thread.Sleep(4000);

            // GetNumberOfDiceMatchingBid
            int numberOfDiceMatchingBid = GetNumberOfDiceMatchingBid(game, previousBid.Pips);

            if (numberOfDiceMatchingBid >= previousBid.Quantity)
            {
                // there are more matching dice than previous bid (previous bid was good)
                liarCall.IsSuccess = false;

                //determine penalty
                var penalty = (numberOfDiceMatchingBid - previousBid.Quantity) + 1; // if variable penalty
                if (game.Penalty != 0) penalty = game.Penalty; // penalty is set to 0 for variable penalty games

                if (PlayerEligibleForSafeguard(game, playerWhoseTurnItIs.NumberOfDice, penalty))
                {
                    if (game.PenaltyGainDice) penalty = 5 - playerWhoseTurnItIs.NumberOfDice;
                    else penalty = playerWhoseTurnItIs.NumberOfDice - 1;

                    await SendMessageAsync($":shield: Snowball shield activated. :shield:");
                    Thread.Sleep(2000);
                }

                var loses = "loses";
                if (game.PenaltyGainDice) loses = "gains";
                // send outcome of unsuccessful liar call
                await SendMessageAsync($"There was actually `{numberOfDiceMatchingBid}` {biddingName}. :axe: {GetUser(playerWhoseTurnItIs.Player.Username).Mention} {loses} {penalty} dice. :axe:");

                // if matching dice is exactly what previous bid was, send that taunt!
                if (numberOfDiceMatchingBid == previousBid.Quantity)
                {
                    var rattles = _db.Rattles.SingleOrDefault(x => x.Username == previousBid.GamePlayer.Player.Username);
                    if (rattles != null)
                    {
                        await SendMessageAsync(rattles.Tauntrattle);
                    }
                }

                await SendRoundSummaryForBots(game);
                await SendRoundSummary(game);

                await CheckGhostAttempts(game);

                // make player with unsuccessful liar call the next to go
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, playerWhoseTurnItIs, penalty);
            }
            else
            {
                liarCall.IsSuccess = true;

                // do same with penalty as in the if statement
                var penalty = previousBid.Quantity - numberOfDiceMatchingBid;
                if (game.Penalty != 0) penalty = game.Penalty;

                if (PlayerEligibleForSafeguard(game, previousBid.GamePlayer.NumberOfDice, penalty))
                {
                    if (game.PenaltyGainDice) penalty = 5 - previousBid.GamePlayer.NumberOfDice;
                    else penalty = previousBid.GamePlayer.NumberOfDice - 1;

                    await SendMessageAsync($":shield: Snowball shield activated. :shield:");
                    Thread.Sleep(2000);
                }

                var loses = "loses";
                if (game.PenaltyGainDice) loses = "gains";

                await SendMessageAsync($"There was actually `{numberOfDiceMatchingBid}` {biddingName}. :axe: {GetUser(previousBid.GamePlayer.Player.Username).Mention} {loses} {penalty} dice. :axe:");

                await SendRoundSummaryForBots(game);
                await SendRoundSummary(game);

                await CheckGhostAttempts(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.GamePlayer, penalty);
            }
            liarCall.SetDuration();
            _db.Actions.Add(liarCall);
            _db.SaveChanges();

            // wait to start new round
            Thread.Sleep(4000);

            RemoveActiveDeals(game);
            RemovePayupPlayer(game);

            await RollDiceStartNewRoundAsync(game);
        }
    }
}
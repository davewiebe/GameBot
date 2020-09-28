using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("exact")]
        public async Task Exact()
        {
            if (await ValidateState(GameState.InProgress) == false) return;

            var game = GetGame(GameState.InProgress);

            //ghost player rejoin
            var ghosts = GetPlayers(game).Where(x => x.NumberOfDice == 0);
            var ghostPlayer = ghosts.SingleOrDefault(x => x.Username == Context.User.Username);
            if (ghostPlayer != null)
            {
                if (GetPlayers(game).Count == 2) return;
                if (ghostPlayer.GhostAttemptsLeft > 0)
                {
                    var lastBuid = GetMostRecentBid(game);
                    if (lastBuid == null) return;
                    if (lastBuid.Quantity == 0) return;

                    ghostPlayer.GhostAttemptQuantity = lastBuid.Quantity;
                    ghostPlayer.GhostAttemptPips = lastBuid.Pips;
                    ghostPlayer.GhostAttemptsLeft -= 1;
                    _db.SaveChanges();
                    await SendMessage($"{GetUserNickname(Context.User.Username)}'s exact attempt has been recorded. Good luck.");
                }
            }



            var originalBiddingPlayer = GetCurrentPlayer(game);
            if (game.CanCallExactAnytime)
            {
                var player = _db.Players.AsQueryable().Where(x => x.GameId == game.Id).OrderBy(x => x.TurnOrder)
                    .Where(x => x.NumberOfDice > 0)
                    .SingleOrDefault(x => x.Username == Context.User.Username);
                if (player == null) return;

                game.PlayerTurnId = player.Id;
                _db.SaveChanges();
            }

            var biddingPlayer = GetCurrentPlayer(game);

            if (biddingPlayer.Username != Context.User.Username)
            {
                return;
            }

            // Cannot be first bid of the round
            var previousBid = GetMostRecentBid(game);
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;
            int countOfPips = GetNumberOfDiceMatchingBid(game, previousBid.Pips);

            _db.Bids.Add(new Bid
            {
                PlayerId = game.PlayerTurnId.Value,
                Call = "exact",
                GameId = game.Id
            });
            _db.SaveChanges();

            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch
            {
            }

            var bidObject = previousBid.Pips.GetEmoji();
            var bidName = "dice";
            if (game.FaceoffEnabled && GetPlayers(game).Sum(x => x.NumberOfDice) == 2)
            {
                bidObject = ":record_button:";
                bidName = "pips";
            }
            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **exact** on `{previousBid.Quantity}` ˣ {bidObject}.");

            Thread.Sleep(4000);

            if (countOfPips == previousBid.Quantity)
            {
                Thread.Sleep(3000);

                await SendMessage($":zany_face: The madman did it! It was exact! :zany_face:");

                var numPlayersLeft = GetPlayers(game).Where(x => x.NumberOfDice > 0).Count();
                if (game.ExactCallBonus > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
                {
                    biddingPlayer.NumberOfDice += game.ExactCallBonus;
                    if (biddingPlayer.NumberOfDice > game.NumberOfDice) biddingPlayer.NumberOfDice = game.NumberOfDice;
                    _db.SaveChanges();
                    await SendMessage($"\n:crossed_swords: As a bonus, they gain `{game.ExactCallBonus}` dice :crossed_swords:");
                }

                if (game.ExactCallPenalty > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
                {
                    await SendMessage($":crossed_swords: As a bonus, everyone else loses `{game.ExactCallPenalty}` dice :crossed_swords:");

                    var otherplayers = GetPlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != biddingPlayer.Id);
                    foreach (var player in otherplayers)
                    {
                        await DecrementDieFromPlayer(player, game.ExactCallPenalty);
                    }
                }

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await CheckGhostAttempts(game);

                SetTurnPlayerToRoundStartPlayer(game);
            }
            else
            {
                var penalty = Math.Abs(countOfPips - previousBid.Quantity);
                if (game.Penalty != 0) penalty = game.Penalty;

                await SendMessage($"There was actually `{countOfPips}` {bidName}. :fire: {GetUser(biddingPlayer.Username).Mention} loses {penalty} dice. :fire:");

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);

                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer, penalty);
            }

            Thread.Sleep(4000);
            await RollDiceStartNewRound(game);
        }

    }
}
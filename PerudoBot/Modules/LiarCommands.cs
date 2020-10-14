using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("liar")]
        public async Task LiarAsync()
        {
            var game = await GetGameAsync(GameState.InProgress);

            if (game == null)
            {
                await SendMessageAsync($"Cannot do that at this time.");
            }

            Bid previousBid = game.CurrentRound.LastAction as Bid;
            if (previousBid == null) return;

            var liarCall = new LiarCallRequest
            {
                Game = game,
                Caller = _perudoGameService.GetCurrentPlayer(game),
                PreviousBid = previousBid
            };

            var liarCallResult = await _perudoGameService.HandleLiarCallAsync(liarCall);

            if (!string.IsNullOrEmpty(liarCallResult.ErrorMessage))
            {
                await SendMessageAsync(liarCallResult.ErrorMessage);
                return;
            }

            // set bidding style to dice (for everything but faceoff round)
            var biddingObject = previousBid.Pips.GetEmoji();
            var biddingName = "dice";

            if (game.CurrentRound is FaceoffRound)
            {
                biddingObject = ":record_button:";
                biddingName = "pips";
            }

            DeleteCommandFromDiscord();

            // send message that liar has been called, w/ details
            await SendMessageAsync($"{liarCall.Caller.Username} called **liar** on `{previousBid.Quantity}` ˣ {biddingObject}.");

            await PauseForDramaticEffectAsync(3, 5);

            var losingPlayer = (liarCallResult.IsSuccess) ? liarCall.PreviousBid.Player : liarCall.Caller;

            await SendMessageAsync($"There was actually `{liarCallResult.ActualQuantity}` {biddingName}. :fire: {GetUser(losingPlayer.Username).Mention} loses {liarCallResult.Penalty} dice. :fire:");

            if (previousBid.Quantity == liarCallResult.ActualQuantity)
            {
                await SendPlayerTauntRattleAsync(previousBid.Player.Username);
            }

            await SendRoundSummaryForBots(game);
            await SendRoundSummary(game);

            await CheckGhostAttempts(game);

            await DecrementDieFromPlayerAndSetThierTurnAsync(game, losingPlayer, liarCallResult.Penalty);
            await PauseForDramaticEffectAsync(4);
            await RollDiceStartNewRound(game);
        }

        private async Task PauseForDramaticEffectAsync(int? secondsToPause = null)
        {
            if (secondsToPause == null)
            {
                secondsToPause = new Random().Next(3, 5);
            }
            await Task.Delay((int)secondsToPause * 1000);
        }

        private async Task PauseForDramaticEffectAsync(int randomMinValue, int randomMaxValue)
        {
            var secondsToPause = new Random().Next(randomMinValue, randomMaxValue);

            await PauseForDramaticEffectAsync(secondsToPause);
        }

        private async Task SendPlayerTauntRattleAsync(string username)
        {
            var rattles = _db.Rattles.SingleOrDefault(x => x.Username == username);
            if (rattles != null)
            {
                await SendMessageAsync(rattles.Tauntrattle);
            }
        }
    }
}
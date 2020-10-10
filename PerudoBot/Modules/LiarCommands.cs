using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
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
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            // get inprogress game
            var game = await GetGameAsync(GameState.InProgress);

            // get latest bid and make sure not null or 0 (because liar or exact from last round
            //will have quant&pips of 0&0
            Bid previousBid = game.CurrentRound.GetLatestAction() as Bid;
            if (previousBid == null || previousBid.Quantity == 0) return;

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

            // if its faceoff round, and it's enabled, use pips instead
            if (GetPlayers(game).Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                biddingObject = ":record_button:";
                biddingName = "pips";
            }

            DeleteCommandFromDiscord();
            // send message that liar has been called, w/ details
            await SendMessageAsync($"{liarCall.Caller.Username} called **liar** on `{previousBid.Quantity}` ˣ {biddingObject}.");

            // for the dramatic affect
            Thread.Sleep(4000);

            if (liarCallResult.IsSuccess)
            {
                await SendMessageAsync($"There was actually `{liarCallResult.ActualQuantity}` {biddingName}. :fire: {GetUser(previousBid.Player.Username).Mention} loses {liarCallResult.Penalty} dice. :fire:");
            }
            else
            {
                await SendMessageAsync($"There was actually `{liarCallResult.ActualQuantity}` {biddingName}. :fire: {GetUser(liarCall.Caller.Username).Mention} loses {liarCallResult.Penalty} dice. :fire:");

                if (liarCallResult.ActualQuantity == previousBid.Quantity)
                {
                    var rattles = _db.Rattles.SingleOrDefault(x => x.Username == previousBid.Player.Username);
                    if (rattles != null)
                    {
                        await SendMessageAsync(rattles.Tauntrattle);
                    }
                }
            }

            await SendRoundSummaryForBots(game);
            await SendRoundSummary(game);
            await CheckGhostAttempts(game);

            var losingPlayer = (liarCallResult.IsSuccess) ? liarCall.PreviousBid.Player : liarCall.Caller;
            await DecrementDieFromPlayerAndSetThierTurnAsync(game, losingPlayer, liarCallResult.Penalty);

            Thread.Sleep(4000);

            await RollDiceStartNewRound(game);
        }
    }
}
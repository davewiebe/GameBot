using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("liar")]
        public async Task Liar()
        {
            if (await ValidateState(GameState.InProgress) == false) return;

            var game = GetGame(GameState.InProgress);


            var previousBid = GetMostRecentBid(game);
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;


            if (game.CanCallLiarAnytime)
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

            _db.Bids.Add(new Bid
            {
                PlayerId = biddingPlayer.Id,
                Call = "liar",
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

            var biddingObject = previousBid.Pips.GetEmoji();
            var biddingName = "dice";
            if (GetPlayers(game).Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                biddingObject = ":record_button:";
                biddingName = "pips";
            }
            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **liar** on `{previousBid.Quantity}` ˣ {biddingObject}.");

            Thread.Sleep(4000);

            int countOfPips = GetNumberOfDiceMatchingBid(game, previousBid.Pips);
            if (countOfPips >= previousBid.Quantity)
            {
                var penalty = (countOfPips - previousBid.Quantity) + 1;
                if (game.Penalty != 0) penalty = game.Penalty;

                await SendMessage($"There was actually `{countOfPips}` {biddingName}. :fire: {GetUser(biddingPlayer.Username).Mention} loses {penalty} dice. :fire:");

                if (countOfPips == previousBid.Quantity)
                {
                    var rattles = _db.Rattles.SingleOrDefault(x => x.Username == previousBid.Player.Username);
                    if (rattles != null)
                    {
                        await SendMessage(rattles.Tauntrattle);
                    }
                }

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer, penalty);
                await CheckGhostAttempts(game);
            }
            else
            {
                var penalty = previousBid.Quantity - countOfPips;
                if (game.Penalty != 0) penalty = game.Penalty;

                await SendMessage($"There was actually `{countOfPips}` {biddingName}. :fire: {GetUser(previousBid.Player.Username).Mention} loses {penalty} dice. :fire:");

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.Player, penalty);
                await CheckGhostAttempts(game);
            }

            Thread.Sleep(4000);
            await RollDiceStartNewRound(game);
        }
    }
}
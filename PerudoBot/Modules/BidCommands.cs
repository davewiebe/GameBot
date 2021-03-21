using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("bid")]
        [Alias("b")]
        public async Task BidAsync(params string[] bidText)
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            var game = await GetGameAsync(GameState.InProgress);

            var currentPlayer = GetCurrentPlayer(game);
            //// TO DO.. THIS NEEDS TO BE THE ACTIVE PLAYER, AS THE ACTIVE PLAYER WILL BE THE ONE WHO IS FORCED TO BE THEIR TURN.
            var activePlayer = GetActivePlayer(game);

            if (!game.CanBidAnytime
                && activePlayer.Player.Username != Context.User.Username)
            {
                if (!activePlayer.HasActiveDeal) return;
            }

            var biddingPlayer = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Single(x => x.Player.Username == Context.User.Username);

            var numberOfDiceLeft = _perudoGameService.GetGamePlayers(game).Sum(x => x.NumberOfDice);
            if (game.FaceoffEnabled && numberOfDiceLeft == 2)
            {
                await HandleFaceoffBid(bidText, game, biddingPlayer);
                _db.SaveChanges();
                return;
            }

            await HandlePipBid(bidText, game, biddingPlayer);
            _db.SaveChanges();
        }

        private async Task HandleFaceoffBid(string[] bidText, Game game, GamePlayer biddingPlayer)
        {
            int quantity = 0;
            try
            {
                quantity = int.Parse(bidText[0]);
            }
            catch
            {
                return;
            }

            if (quantity < game.LowestPip * 2 || quantity > game.HighestPip * 2) return;

            var bid = new Bid
            {
                Pips = 0,
                Quantity = quantity,
                Round = game.CurrentRound,
                GamePlayer = biddingPlayer,
                ParentAction = game.CurrentRound.LatestAction,
                GamePlayerRound = biddingPlayer.CurrentGamePlayerRound
            };

            if (await VerifyBid(bid) == false) return;
            /// MONKEY... working here.
            /// TODO: Add Exact bid too
            /// TODO: Add Liar bid too
            ///
            _db.Actions.Add(bid);
            _db.SaveChanges();

            var currentPlayer = GetCurrentPlayer(game);

            var activePlayer = GetActivePlayer(game);
            if (activePlayer.HasActiveDeal && Context.User.Id != activePlayer.Player.UserId)
            {
                biddingPlayer.PendingUserDealIds = $"{biddingPlayer.PendingUserDealIds},{activePlayer.Id}";

                await SendMessageAsync($":money_mouth: {biddingPlayer.Player.Nickname} has accepted the deal! {biddingPlayer.Player.Nickname}, on your turn use `!payup @{activePlayer.Player.Nickname}` to force them to take your turn for you.");
            }
            RemoveActiveDeals(game);
            RemovePayupPlayer(game);

            SetNextPlayer(game, currentPlayer);

            var nextPlayer = GetCurrentPlayer(game);

            DeleteCommandFromDiscord();

            var bidderNickname = biddingPlayer.Player.Nickname;
            var nextPlayerMention = GetUser(nextPlayer.Player.Username).Mention;

            var dealer = "";
            if (currentPlayer.Id != biddingPlayer.Id) dealer = $" (bidding for {currentPlayer.Player.Nickname})";

            var userMessage = $"{ bidderNickname }{dealer} bids `{ quantity}` ˣ :record_button:. { nextPlayerMention } is up.";

            await SendMessageAsync(userMessage);

            await CheckIfNextPlayerHasAutoLiarSetAsync(nextPlayer);
        }

        private async Task HandlePipBid(string[] bidText, Game game, GamePlayer biddingPlayer)
        {
            int quantity = 0;
            int pips = 0;
            try
            {
                quantity = int.Parse(bidText[0]);
                pips = int.Parse(bidText[1].Trim('s'));
            }
            catch
            {
                return;
            }

            if (quantity <= 0) return;
            if (pips < game.LowestPip || pips > game.HighestPip) return;

            var bid = new Bid
            {
                Pips = pips,
                Quantity = quantity,
                GamePlayer = biddingPlayer,
                Round = game.CurrentRound,
                ParentAction = game.CurrentRound.LatestAction,
                GamePlayerRound = biddingPlayer.CurrentGamePlayerRound,
                IsSuccess = true
            };

            if (await VerifyBid(bid) == false) return;

            var currentPlayer2 = GetCurrentPlayer(game);

            if (game.CanBidAnytime && currentPlayer2.Player.Username != Context.User.Username)
            {
                var prevCurrentPlayer = GetCurrentPlayer(game);

                var currentPlayer = _perudoGameService.GetGamePlayers(game)
                    .Where(x => x.NumberOfDice > 0)
                    .SingleOrDefault(x => x.Player.Username == Context.User.Username);
                if (currentPlayer == null) return;
                game.PlayerTurnId = currentPlayer.Id;

                // reset turn order
                var players = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != currentPlayer.Id).ToList();

                var insertIndex = players.FindIndex(x => x.Id == prevCurrentPlayer.Id);

                players.Insert(insertIndex, currentPlayer);
                var order = 0;
                foreach (var player in players)
                {
                    player.TurnOrder = order;
                    order += 1;
                }

                _db.SaveChanges();
            }
            bid.SetDuration();

            _db.Bids.Add(bid);

            _db.SaveChanges();

            var activePlayer = GetActivePlayer(game);
            if (activePlayer.HasActiveDeal && Context.User.Id != activePlayer.Player.UserId)
            {
                biddingPlayer.PendingUserDealIds = $"{biddingPlayer.PendingUserDealIds},{activePlayer.Id}";

                await SendMessageAsync($":money_mouth: {biddingPlayer.Player.Nickname} has accepted the deal! {biddingPlayer.Player.Nickname}, on your turn use `!payup @{activePlayer.Player.Nickname}` to force them to take your turn for you.");
            }
            RemoveActiveDeals(game);
            RemovePayupPlayer(game);

            SetNextPlayer(game, currentPlayer2);

            var nextPlayer = GetCurrentPlayer(game);

            DeleteCommandFromDiscord();

            var bidderNickname = biddingPlayer.Player.Nickname;
            var nextUser = GetUser(nextPlayer.Player.Username);
            var nextPlayerMention = nextUser.Mention;
            
            var snowflakeRound = "";
            if (game.CurrentRound is PalificoRound) snowflakeRound = ":four_leaf_clover: ";
            var dealer = "";
            if (currentPlayer2.Id != biddingPlayer.Id) dealer = $" (bidding for {currentPlayer2.Player.Nickname})";

            var userMessage = $"{snowflakeRound}{ bidderNickname }{dealer} bids `{ quantity}` ˣ { pips.GetEmoji()}. { nextPlayerMention } is up.";

            IUserMessage sentMessage;

            sentMessage = await SendMessageAsync(userMessage);

            if (AreBotsInGame(game)) {
                var botMessage = new {
                    nextPlayer = nextUser.Id.ToString(),
                    diceCount = _perudoGameService.GetGamePlayers(game).Sum(x => x.NumberOfDice),
                    round = game.Rounds.Count,
                    action = BidToActionIndex(bid),
                };

                await SendMessageAsync($"||`@bots update {JsonConvert.SerializeObject(botMessage)}`||");
            }

            bid.MessageId = sentMessage.Id;

            activePlayer.HasActiveDeal = false;
            _db.SaveChanges();

            await CheckIfNextPlayerHasAutoLiarSetAsync(nextPlayer);
        }

        private async Task CheckIfNextPlayerHasAutoLiarSetAsync(GamePlayer nextPlayer)
        {
            if (nextPlayer.CurrentGamePlayerRound.IsAutoLiarSet)
            {
                Thread.Sleep(1000);
                await SendMessageAsync($":hatching_chick: Auto **liar** activated.");
                Thread.Sleep(2000);
                await LiarAsync();
            }
        }

        private async Task<bool> VerifyBid(Bid bid)
        {
            var game = await GetGameAsync(GameState.InProgress);
            var mostRecentBid = GetMostRecentBid(game);

            var players = _perudoGameService.GetGamePlayers(game);

            if (game.FaceoffEnabled && players.Sum(x => x.NumberOfDice) == 2)
            {
                if (mostRecentBid == null) return true;
                if (bid.Quantity <= mostRecentBid.Quantity)
                {
                    await SendMessageAsync("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Quantity > players.Sum(x => x.NumberOfDice))
            {
                await SendMessageAsync($"Really? {bid.Quantity} dice?");
                await SendMessageAsync($"I'm gonna let you think this one through a little bit first.");
                return false;
            }

            if (game.CurrentRound is PalificoRound)
            {
                if (game.CurrentRound.Actions.Count == 0) return true;
                if (bid.GamePlayer.NumberOfDice != 1 && bid.Pips != mostRecentBid.Pips)
                {
                    await SendMessageAsync("Only players at 1 die can change pips in a Palifico round.");
                    return false;
                }

                //if (bid.Quantity < mostRecentBid.Quantity)
                //{
                //    await SendMessageAsync("Bid has to be higher.");
                //    return false;
                //}
                //if (bid.Quantity == mostRecentBid.Quantity && bid.Pips <= mostRecentBid.Pips)
                //{
                //    await SendMessageAsync("Bid has to be higher.");
                //    return false;
                //}
                //return true;
            }

            if (game.WildsEnabled == false && bid.Pips == 1)
            {
                await SendMessageAsync("Cannot bid on wilds this game.");
                return false;
            }

            // first bid of the round
            if (mostRecentBid == null)
            {
                if (bid.Pips == 1)
                {
                    await SendMessageAsync("Cannot start the round by bidding on wilds.");
                    return false;
                }
                return true;
            }

            // If last bid was 1s
            if (bid.Pips == 1 && mostRecentBid.Pips == 1)
            {
                if (bid.Quantity < mostRecentBid.Quantity)
                {
                    await SendMessageAsync("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Pips != 1 && mostRecentBid.Pips == 1)
            {
                if (bid.Quantity < mostRecentBid.Quantity * 2)
                {
                    await SendMessageAsync("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Pips == 1 && mostRecentBid.Pips != 1)
            {
                if (bid.Quantity * 2 <= mostRecentBid.Quantity)
                {
                    await SendMessageAsync("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Quantity < mostRecentBid.Quantity)
            {
                await SendMessageAsync("Bid has to be higher.");
                return false;
            }

            if (bid.Quantity == mostRecentBid.Quantity && bid.Pips <= mostRecentBid.Pips)
            {
                await SendMessageAsync("Bid has to be higher.");
                return false;
            }
            return true;
        }

        // Unwrap bid to it's action index where 0:1x2, 1:1x3, 2:1x4, etc.
        private int BidToActionIndex(Bid bid) {
            if (bid.Pips != 1) 
            {
                int nonWildcard = ((bid.Quantity - 1) * 5);
                int wildcard = bid.Quantity / 2;
                return nonWildcard + wildcard + (bid.Pips - 2);
            }
            else
            {
                // starting at 5, every 11 actions there is a wildcard action
                return 5 + ((bid.Quantity - 1) * 11); 
            }
        }

        private Bid GetMostRecentBid(Game game)
        {
            return game.CurrentRound
                .Actions.OfType<Bid>()
                .LastOrDefault();
        }
    }
}
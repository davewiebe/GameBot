using Discord.Commands;
using Newtonsoft.Json;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("b")]
        public async Task B(params string[] bidText)
        {
            await Bid(bidText);
        }

        [Command("bid")]
        public async Task Bid(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);


            var currentPlayer = GetCurrentPlayer(game);

            if (!game.CanBidAnytime && currentPlayer.Username != Context.User.Username)
            {
                return;
            }

            var biddingPlayer = GetPlayers(game).Where(x => x.NumberOfDice > 0).Single(x => x.Username == Context.User.Username);

            var numberOfDiceLeft = GetPlayers(game).Sum(x => x.NumberOfDice);
            if (game.FaceoffEnabled && numberOfDiceLeft == 2)
            {
                await HandleFaceoffBid(bidText, game, biddingPlayer);
                return;
            }

            await HandlePipBid(bidText, game, biddingPlayer);
        }


        private async Task HandleFaceoffBid(string[] bidText, Data.Game game, Player biddingPlayer)
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

            if (quantity < 2 || quantity > 12) return;


            var bid = new Bid
            {
                Call = "",
                Pips = 0,
                Quantity = quantity,
                PlayerId = biddingPlayer.Id,
                GameId = game.Id
            };

            if (await VerifyBid(bid) == false) return;
            /// MONKEY... working here.
            /// TODO: Add Exact bid too
            /// TODO: Add Liar bid too
            /// 
            _db.Bids.Add(bid);
            _db.SaveChanges();

            SetNextPlayer(game, biddingPlayer);

            var nextPlayer = GetCurrentPlayer(game);

            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch
            {
            }
            var bidderNickname = GetUserNickname(biddingPlayer.Username);
            var nextPlayerMention = GetUser(nextPlayer.Username).Mention;

            var userMessage = $"{ bidderNickname } bids `{ quantity}` ˣ :record_button:. { nextPlayerMention } is up.";

            await SendMessage(userMessage);
        }


        private async Task HandlePipBid(string[] bidText, Data.Game game, Player biddingPlayer)
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
            if (pips < 1 || pips > 6) return;


            var bid = new Bid
            {
                Call = "",
                Pips = pips,
                Quantity = quantity,
                Player = biddingPlayer,
                GameId = game.Id
            };

            if (await VerifyBid(bid) == false) return;


            if (game.CanBidAnytime && GetCurrentPlayer(game).Username != Context.User.Username)
            {
                var prevCurrentPlayer = GetCurrentPlayer(game);

                var currentPlayer = GetPlayers(game)
                    .Where(x => x.NumberOfDice > 0)
                    .SingleOrDefault(x => x.Username == Context.User.Username);
                if (currentPlayer == null) return;
                game.PlayerTurnId = currentPlayer.Id;

                // reset turn order
                var players = GetPlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != currentPlayer.Id).ToList();

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




            _db.Bids.Add(bid);
            _db.SaveChanges();

            SetNextPlayer(game, biddingPlayer);

            var nextPlayer = GetCurrentPlayer(game);

            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch
            {
            }
            var bidderNickname = GetUserNickname(biddingPlayer.Username);
            var nextPlayerMention = GetUser(nextPlayer.Username).Mention;

            var userMessage = $"{ bidderNickname } bids `{ quantity}` ˣ { pips.GetEmoji()}. { nextPlayerMention } is up.";

            var botMessage = new
            {
                U = bidderNickname,
                P = pips,
                Q = quantity
            };

            if (AreBotsInGame(game))
            {
                await SendMessage($"{userMessage} ||{JsonConvert.SerializeObject(botMessage)}||");
            }
            else
            {
                await SendMessage(userMessage);
            }
        }

        private async Task<bool> VerifyBid(Bid bid)
        {
            var game = GetGame(IN_PROGRESS);
            Bid mostRecentBid = GetMostRecentBid(game);

            var players = GetPlayers(game);

            if (game.FaceoffEnabled && players.Sum(x => x.NumberOfDice) == 2)
            {
                if (mostRecentBid == null) return true;
                if (bid.Quantity < mostRecentBid.Quantity)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                return true;
            }


            if (bid.Quantity > players.Sum(x => x.NumberOfDice))
            {
                await SendMessage($"Really? {bid.Quantity} dice?");
                await SendMessage($"I'm gonna let you think this one through a little bit first.");
                return false;
            }

            if (game.NextRoundIsPalifico)
            {
                if (bid.Player.NumberOfDice != 1 && bid.Pips != mostRecentBid.Pips)
                {
                    await SendMessage("Only players at 1 die can change pips in Palifico round.");
                    return false;
                }

                if (bid.Quantity < mostRecentBid.Quantity)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                if (bid.Quantity == mostRecentBid.Quantity && bid.Pips <= mostRecentBid.Pips)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (game.WildsEnabled == false && bid.Pips == 1)
            {
                await SendMessage("Cannot bid on wilds this game.");
                return false;
            }

            if (mostRecentBid == null)
            {
                if (bid.Pips == 1)
                {
                    await SendMessage("Cannot start the round by bidding on wilds.");
                    return false;
                }
                return true;
            }

            if (mostRecentBid.Call != "")
            {
                if (bid.Pips == 1)
                {
                    await SendMessage("Cannot start the round by bidding on wilds.");
                    return false;
                }
                return true;
            }

            // If last bid was 1s
            if (bid.Pips == 1 && mostRecentBid.Pips == 1)
            {
                if (bid.Quantity < mostRecentBid.Quantity)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Pips != 1 && mostRecentBid.Pips == 1)
            {
                if (bid.Quantity < mostRecentBid.Quantity * 2)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Pips == 1 && mostRecentBid.Pips != 1)
            {
                var prevRoundId = 0;
                var prevRound = _db.Bids.AsQueryable().Where(x => x.Call != "").ToList().LastOrDefault();

                if (prevRound != null) prevRoundId = prevRound.Id;

                // Removed this. Apparently not in the rules
                //var hasGoneToOnesAlready = _db.Bids.AsQueryable()
                //    .Where(x => x.Id > prevRoundId)
                //    .Where(x => x.GameId == game.Id).Where(x => x.Pips == 1).Any();
                //if (hasGoneToOnesAlready)
                //{
                //    await SendMessage("Cannot switch to wilds more than once a round.");
                //    return false;
                //}


                if (bid.Quantity * 2 <= mostRecentBid.Quantity)
                {
                    await SendMessage("Bid has to be higher.");
                    return false;
                }
                return true;
            }

            if (bid.Quantity < mostRecentBid.Quantity)
            {
                await SendMessage("Bid has to be higher.");
                return false;
            }
            if (bid.Quantity == mostRecentBid.Quantity && bid.Pips <= mostRecentBid.Pips)
            {
                await SendMessage("Bid has to be higher.");
                return false;
            }
            return true;
        }

        private Bid GetMostRecentBid(Data.Game game)
        {
            return _db.Bids.AsQueryable().Where(x => x.GameId == game.Id).ToList().LastOrDefault();
        }
    }
}

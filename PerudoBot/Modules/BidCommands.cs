using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System.Linq;
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

        private async Task HandleFaceoffBid(string[] bidText, Game game, Player biddingPlayer)
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

            if (quantity < game.LowestPip*2 || quantity > game.HighestPip*2 ) return;

            var bid = new Bid
            {
                Pips = 0,
                Quantity = quantity,
                PlayerId = biddingPlayer.Id,
                RoundId = game.GetLatestRound().Id
            };

            if (await VerifyBid(bid) == false) return;
            /// MONKEY... working here.
            /// TODO: Add Exact bid too
            /// TODO: Add Liar bid too
            ///
            _db.Actions.Add(bid);
            _db.SaveChanges();

            SetNextPlayer(game, biddingPlayer);

            var nextPlayer = GetCurrentPlayer(game);

            DeleteCommandFromDiscord();
            var bidderNickname = GetUserNickname(biddingPlayer.Username);
            var nextPlayerMention = GetUser(nextPlayer.Username).Mention;

            var userMessage = $"{ bidderNickname } bids `{ quantity}` ˣ :record_button:. { nextPlayerMention } is up.";

            await SendMessageAsync(userMessage);
        }

        private async Task HandlePipBid(string[] bidText, Game game, Player biddingPlayer)
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
                Player = biddingPlayer,
                RoundId = game.GetLatestRound().Id,
                IsSuccess = true
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

            DeleteCommandFromDiscord();
            var bidderNickname = GetUserNickname(biddingPlayer.Username);
            var nextPlayerMention = GetUser(nextPlayer.Username).Mention;

            var userMessage = $"{ bidderNickname } bids `{ quantity}` ˣ { pips.GetEmoji()}. { nextPlayerMention } is up.";
            IUserMessage sentMessage;

            if (AreBotsInGame(game))
            {
                var botMessage = new
                {
                    U = bidderNickname,
                    P = pips,
                    Q = quantity
                };
                sentMessage = await SendMessageAsync($"{userMessage} || {JsonConvert.SerializeObject(botMessage)}||");
            }
            else
            {
                sentMessage = await SendMessageAsync(userMessage);
            }

            bid.MessageId = sentMessage.Id;
            _db.SaveChanges();
        }

        private async Task<bool> VerifyBid(Bid bid)
        {
            var game = await GetGameAsync(GameState.InProgress);
            var mostRecentBid = GetMostRecentBid(game);

            var players = GetPlayers(game);

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

            if (game.GetLatestRound() is PalificoRound)
            {
                if (game.GetLatestRound().Actions.Count == 0) return true;
                if (bid.Player.NumberOfDice != 1 && bid.Pips != mostRecentBid.Pips)
                {
                    await SendMessageAsync("Only players at 1 die can change pips in Palifico round.");
                    return false;
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

            if (game.WildsEnabled == false && bid.Pips == 1)
            {
                await SendMessageAsync("Cannot bid on wilds this game.");
                return false;
            }

            if (mostRecentBid == null)
            {
                if (bid.Pips == 1)
                {
                    await SendMessageAsync("Cannot start the round by bidding on wilds.");
                    return false;
                }
                return true;
            }

            //if (mostRecentBid is Bid) /// not a bid maybe?
            //{
            //    if (bid.Pips == 1)
            //    {
            //        await SendMessageAsync("Cannot start the round by bidding on wilds.");
            //        return false;
            //    }
            //    return true;
            //}

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
                var prevRoundId = 0;
                var prevRound = _db.Bids.AsQueryable().ToList().LastOrDefault();

                if (prevRound != null) prevRoundId = prevRound.Id;

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

        private Bid GetMostRecentBid(Game game)
        {
            return game.GetLatestRound()
                .Actions.OfType<Bid>()
                .LastOrDefault();
        }
    }
}
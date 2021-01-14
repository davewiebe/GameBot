using Discord;
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
        [Alias("nice")]
        public async Task Exact()
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            var game = await GetGameAsync(GameState.InProgress);

            //ghost player rejoin
            var ghosts = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice == 0);
            var ghostPlayer = ghosts.SingleOrDefault(x => x.Player.Username == Context.User.Username);
            if (ghostPlayer != null)
            {
                if (_perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Count() == 2) return;
                if (ghostPlayer.GhostAttemptsLeft > 0 && ghostPlayer.GhostAttemptPips == 0)
                {
                    var lastBid = GetMostRecentBid(game);
                    if (lastBid == null) return;
                    if (lastBid.Quantity == 0) return;

                    ghostPlayer.GhostAttemptQuantity = lastBid.Quantity;
                    ghostPlayer.GhostAttemptPips = lastBid.Pips;
                    _db.SaveChanges();

                    var lastBidMessage = await Context.Channel.GetMessageAsync(lastBid.MessageId);

                    try
                    {
                        _ = Task.Run(() => Context.Message.DeleteAsync());
                    }
                    catch { }
                    try
                    {
                        var castedMessage = lastBidMessage as IUserMessage;

                        await castedMessage.ModifyAsync(msg => msg.Content = $"{castedMessage.Content} :ghost: {GetUserNickname(Context.User.Username)}!");
                    }
                    catch
                    {
                        try
                        {
                            _ = Task.Run(() => lastBidMessage.DeleteAsync());
                        }
                        catch { }

                        var newMessage = await SendMessageAsync($"{lastBidMessage.Content} :ghost: {GetUserNickname(Context.User.Username)} (old)");
                        lastBid.MessageId = newMessage.Id;
                        _db.SaveChanges();
                    }

                    return;
                }
            }

            // Cannot be first bid of the round
            var previousBid = GetMostRecentBid(game);
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;
            int countOfPips = GetNumberOfDiceMatchingBid(game, previousBid.Pips);

            var isOutOfTurn = false;
            var originalBiddingPlayer = GetCurrentPlayer(game);
            if (game.CanCallExactAnytime)
            {
                var player = _db.GamePlayers.AsQueryable().Where(x => x.GameId == game.Id).OrderBy(x => x.TurnOrder)
                    .Where(x => x.NumberOfDice > 0)
                    .SingleOrDefault(x => x.Player.Username == Context.User.Username);
                if (player == null) return;

                isOutOfTurn = true;

                game.PlayerTurnId = player.Id;
                _db.SaveChanges();
            }

            var currentPlayer = GetCurrentPlayer(game);
            var activePlayer = GetActivePlayer(game);
            var exactingPlayer = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Single(x => x.Player.Username == Context.User.Username);

            if (currentPlayer.Id != exactingPlayer.Id)
            {
                if(!activePlayer.HasActiveDeal) return;
            }

            if (activePlayer.HasActiveDeal && Context.User.Id != activePlayer.Player.UserId)
            {
                exactingPlayer.PendingUserDealIds = $"{exactingPlayer.PendingUserDealIds},{activePlayer.Id}";

                await SendMessageAsync($":money_mouth: {exactingPlayer.Player.Nickname} has accepted the deal! {exactingPlayer.Player.Nickname}, on your turn use `!payup @{activePlayer.Player.Nickname}` to force them to take your turn for you.");
            }

            var exactCall = new ExactCall
            {
                GamePlayer = exactingPlayer,
                Round = game.CurrentRound,
                GamePlayerRound = exactingPlayer.CurrentGamePlayerRound,
                ParentAction = previousBid,
                IsOutOfTurn = isOutOfTurn,
                IsSuccess = false
            };
            exactCall.SetDuration();
            _db.Actions.Add(exactCall);

            DeleteCommandFromDiscord();

            var bidObject = previousBid.Pips.GetEmoji();
            var bidName = "dice";
            if (game.FaceoffEnabled && _perudoGameService.GetGamePlayers(game).Sum(x => x.NumberOfDice) == 2)
            {
                bidObject = ":record_button:";
                bidName = "pips";
            }

            var dealer = "";
            if (currentPlayer.Id != exactingPlayer.Id) dealer = $" ({currentPlayer.Player.Nickname})";

            await SendMessageAsync($"{exactingPlayer.Player.Nickname}{dealer} called **exact** on `{previousBid.Quantity}` ˣ {bidObject}.");

            Thread.Sleep(4000);

            if (countOfPips == previousBid.Quantity)
            {
                Thread.Sleep(3000);

                await SendMessageAsync($":snowboarder: The madman did it! It was exact! :snowboarder:");

                exactCall.IsSuccess = true;

                var numPlayersLeft = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Count();
                if (game.ExactCallBonus > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != exactingPlayer.Id)
                {
                    exactingPlayer.NumberOfDice += game.ExactCallBonus;
                    exactingPlayer.CurrentGamePlayerRound.Penalty = -1;
                    if (exactingPlayer.NumberOfDice > game.NumberOfDice) exactingPlayer.NumberOfDice = game.NumberOfDice;
                    _db.SaveChanges();
                    await SendMessageAsync($"\n:crossed_swords: As a bonus, they gain `{game.ExactCallBonus}` dice :crossed_swords:");
                }

                if (game.ExactCallPenalty > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != exactingPlayer.Id)
                {
                    await SendMessageAsync($":crossed_swords: As a bonus, everyone else loses `{game.ExactCallPenalty}` dice :crossed_swords:");

                    await SendRoundSummaryForBots(game);
                    await SendRoundSummary(game);
                    await CheckGhostAttempts(game);

                    var otherplayers = _perudoGameService.GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != exactingPlayer.Id);
                    foreach (var player in otherplayers)
                    {
                        await DecrementDieFromPlayer(player, game.ExactCallPenalty);
                    }
                }
                else
                {
                    await SendRoundSummaryForBots(game);
                    await SendRoundSummary(game);
                    await CheckGhostAttempts(game);
                }

                SetTurnPlayerToRoundStartPlayer(game);
            }
            else
            {
                var penalty = Math.Abs(countOfPips - previousBid.Quantity);
                if (game.Penalty != 0) penalty = game.Penalty;

                if (PlayerEligibleForSafeguard(game, exactingPlayer.NumberOfDice, penalty))
                {
                    if (game.PenaltyGainDice) penalty = 5 - exactingPlayer.NumberOfDice;
                    else penalty = exactingPlayer.NumberOfDice - 1;

                    await SendMessageAsync($":shield: Snowball shield activated. :shield:");
                    Thread.Sleep(2000);
                }

                var loses = "loses";
                if (game.PenaltyGainDice) loses = "gains";
                await SendMessageAsync($"There was actually `{countOfPips}` {bidName}. :candle: {GetUser(exactingPlayer.Player.Username).Mention} {loses} {penalty} dice. :candle:");

                await SendRoundSummaryForBots(game);
                await SendRoundSummary(game);
                await CheckGhostAttempts(game);

                await DecrementDieFromPlayerAndSetThierTurnAsync(game, exactingPlayer, penalty);
            }

            RemoveActiveDeals(game);
            RemovePayupPlayer(game);

            Thread.Sleep(4000);
            await RollDiceStartNewRoundAsync(game);
        }
    }
}
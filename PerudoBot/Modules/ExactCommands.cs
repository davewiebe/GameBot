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
        public async Task Exact()
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            var game = await GetGameAsync(GameState.InProgress);

            //ghost player rejoin
            var ghosts = GetPlayers(game).Where(x => x.NumberOfDice == 0);
            var ghostPlayer = ghosts.SingleOrDefault(x => x.Player.Username == Context.User.Username);
            if (ghostPlayer != null)
            {
                if (GetPlayers(game).Where(x => x.NumberOfDice > 0).Count() == 2) return;
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

            var biddingPlayer = GetCurrentPlayer(game);

            if (biddingPlayer.Player.Username != Context.User.Username)
            {
                return;
            }

            var exactCall = new ExactCall
            {
                GamePlayerId = game.PlayerTurnId.Value,
                RoundId = game.GetLatestRound().Id,
                ParentActionId = previousBid.Id,
                IsOutOfTurn = isOutOfTurn,
                IsSuccess = false
            };

            _db.Actions.Add(exactCall);

            DeleteCommandFromDiscord();

            var bidObject = previousBid.Pips.GetEmoji();
            var bidName = "dice";
            if (game.FaceoffEnabled && GetPlayers(game).Sum(x => x.NumberOfDice) == 2)
            {
                bidObject = ":record_button:";
                bidName = "pips";
            }
            await SendMessageAsync($"{biddingPlayer.Player.Nickname} called **exact** on `{previousBid.Quantity}` ˣ {bidObject}.");

            Thread.Sleep(4000);

            if (countOfPips == previousBid.Quantity)
            {
                Thread.Sleep(3000);

                await SendMessageAsync($":zany_face: The madman did it! It was exact! :zany_face:");

                exactCall.IsSuccess = true;

                var numPlayersLeft = GetPlayers(game).Where(x => x.NumberOfDice > 0).Count();
                if (game.ExactCallBonus > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
                {
                    biddingPlayer.NumberOfDice += game.ExactCallBonus;
                    if (biddingPlayer.NumberOfDice > game.NumberOfDice) biddingPlayer.NumberOfDice = game.NumberOfDice;
                    _db.SaveChanges();
                    await SendMessageAsync($"\n:crossed_swords: As a bonus, they gain `{game.ExactCallBonus}` dice :crossed_swords:");
                }

                if (game.ExactCallPenalty > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
                {
                    await SendMessageAsync($":crossed_swords: As a bonus, everyone else loses `{game.ExactCallPenalty}` dice :crossed_swords:");

                    await SendRoundSummaryForBots(game);
                    await SendRoundSummary(game);
                    await CheckGhostAttempts(game);

                    var otherplayers = GetPlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != biddingPlayer.Id);
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

                await SendMessageAsync($"There was actually `{countOfPips}` {bidName}. :fire: {GetUser(biddingPlayer.Player.Username).Mention} loses {penalty} dice. :fire:");

                await SendRoundSummaryForBots(game);
                await SendRoundSummary(game);
                await CheckGhostAttempts(game);

                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer, penalty);
            }

            Thread.Sleep(4000);
            await RollDiceStartNewRound(game);
        }
    }
}
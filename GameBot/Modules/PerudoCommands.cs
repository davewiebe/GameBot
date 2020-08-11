using Discord;
using Discord.Commands;
using GameBot.Data;
using GameBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace GameBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int SETUP = 0;
        private const int IN_PROGRESS = 1;
        private const int ENDED = 2;

        [Command("new")]
        public async Task NewGame()
        {
            if (_botType != "perudo") return;

            if (_db.Games.SingleOrDefault(x => x.State == IN_PROGRESS || x.State == SETUP) != null)
            {
                await ReplyAsync($"A game is already in progress.");
                return;
            }

            _db.Games.Add(new Data.Game
            {
                State = 0
            });
            _db.SaveChanges();

            await ReplyAsync($"New game created. Add players with \"!add @user\".");
        }

        [Command("add")]
        public async Task AddUserToGame(string user)
        {
            if (_botType != "perudo") return;

            var userToAdd = Context.Message.MentionedUsers.First();

            var game = _db.Games.SingleOrDefault(x => x.State == SETUP);
            if (game == null)
            {
                await ReplyAsync($"Unable to add players at this time.");
                return;
            }

            //TODO: add check for adding same player twice.
            var players = await GetPlayersAsync(game);
            if (players.FirstOrDefault(x => x.Username == userToAdd.Username) != null)
            {
                await ReplyAsync($"{userToAdd.Username} is already in the game.");
                return;
            }


            _db.Players.Add(new Player
            {
                GameId = game.Id,
                Username = userToAdd.Username
            });

            _db.SaveChanges();

            await ReplyAsync($"{userToAdd.Username} added to game. Start the game with \"!start\"");
        }

        [Command("remove")]
        public async Task RemoveUserFromGame(string user)
        {
            if (_botType != "perudo") return;

            var userToAdd = Context.Message.MentionedUsers.First();

            var game = _db.Games.SingleOrDefault(x => x.State == SETUP);
            if (game == null)
            {
                await ReplyAsync($"Unable to remove players at this time.");
                return;
            }

            var userToRemove = _db.Players.FirstOrDefault(x => x.GameId == game.Id && x.Username == userToAdd.Username);

            _db.Players.Remove(userToRemove);
            _db.SaveChanges();

            await ReplyAsync($"{userToAdd.Username} added to game. Start the game with \"!start\"");
        }

        private async Task<bool> ValidateState(int stateId)
        {
            if (_botType != "perudo") return false;
            var game = await GetGameAsync(stateId);

            if (game == null)
            {
                await ReplyAsync($"Cannot do that at this time.");
                return false;
            }
            return true;
        }

        private async Task<Data.Game> GetGameAsync(int stateId)
        {
            return await _db.Games.AsQueryable().SingleOrDefaultAsync(x => x.State == stateId);
        }

        [Command("start")]
        public async Task Start()
        {
            if (await ValidateState(SETUP) == false) return;

            var game = await GetGameAsync(SETUP);

            var players = await GetPlayersAsync(game);

            game.State = IN_PROGRESS;
            game.PlayerTurnId = players.First().Id;

            await RollDice(game);

            _db.SaveChanges();

            await ReplyAsync($"Starting the game!");
            await ReplyAsync($"Use \"!bid 2 2s\" or \"!exact\" or \"!liar\" to play.");
        }

        private async Task<List<Player>> GetPlayersAsync(Data.Game game)
        {
            return await _db.Players.AsQueryable().Where(x => x.GameId == game.Id).ToListAsync();
        }

        private async Task RollDice(Data.Game game)
        {
            // IF THERE IS ONLY ONE PLAYER LEFT, ANNOUNCE THAT THEY WIN
            List<Player> lists = (await GetPlayersAsync(game));

            var activePlayers = lists.Where(x => x.NumberOfDice > 0);
            if (activePlayers.Count() == 1)
            {
                await ReplyAsync($"{activePlayers.Single().Username} is the winner with {activePlayers.Single().NumberOfDice} dice remaining!");

                game.State = ENDED;
                _db.SaveChanges();
                return;
            }


            var r = new Random();

            var players2 = _db.Players.AsQueryable().Where(x => x.GameId == game.Id).ToList();

            foreach (var player in players2)
            {
                if (player.NumberOfDice >= 1) player.Die1 = r.Next(1, 7); else player.Die1 = null;
                if (player.NumberOfDice >= 2) player.Die2 = r.Next(1, 7); else player.Die2 = null;
                if (player.NumberOfDice >= 3) player.Die3 = r.Next(1, 7); else player.Die3 = null;
                if (player.NumberOfDice >= 4) player.Die4 = r.Next(1, 7); else player.Die4 = null;
                if (player.NumberOfDice >= 5) player.Die5 = r.Next(1, 7); else player.Die5 = null;

                var user = Context.Guild.Users.Single(x => x.Username == player.Username);
                await user.SendMessageAsync($"Your dice: {player.Die1} {player.Die2} {player.Die3} {player.Die4} {player.Die5}");
            }
            _db.SaveChanges();

            await ReplyAsync("A new round has begun. Your dice have been messaged to you.");

            var players = await GetPlayersAsync(game);
            var playerDiceLeft = players.Select(x => $"{x.Username} - {x.NumberOfDice}");
            var totalDice = players.Sum(x => x.NumberOfDice);

            await ReplyAsync($"Current standings: {string.Join(",", playerDiceLeft)}. Total Dice: {totalDice}.");

            await ReplyAsync($"{GetCurrentPlayer(game).Username} goes first.");
        }

        [Command("terminate")]
        public async Task Terminate(params string[] bidText)
        {
            if (_botType != "perudo") return;

            var game = await GetGameAsync(IN_PROGRESS);
            if (game != null) game.State = ENDED;

            game = await GetGameAsync(SETUP);
            if (game != null) game.State = ENDED;

            _db.SaveChanges();

            await ReplyAsync("I'll be back.");
        }


        [Command("bid")]
        public async Task Bid(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = await GetGameAsync(IN_PROGRESS);

            var biddingPlayer = GetBiddingPlayer(game);

            if (biddingPlayer.Username != Context.User.Username)
            {
                return;
            }

            await HandlePipBid(bidText, game, biddingPlayer);
        }


        [Command("exact")]
        public async Task Exact(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = await GetGameAsync(IN_PROGRESS);

            var biddingPlayer = GetBiddingPlayer(game);

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

            await DisplayAllDice(game);

            if (countOfPips == previousBid.Quantity)
            {
                await ReplyAsync("The madman did it! It was exact!");
            }
            else
            {
                await ReplyAsync($"There was actually {countOfPips} dice");
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, GetCurrentPlayer(game));
            }

            await RollDice(game);
        }

        [Command("liar")]
        public async Task Liar(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = await GetGameAsync(IN_PROGRESS);

            var biddingPlayer = GetBiddingPlayer(game);

            if (biddingPlayer.Username != Context.User.Username)
            {
                return;
            }


            var previousBid = GetMostRecentBid(game);
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;
            int countOfPips = GetNumberOfDiceMatchingBid(game, previousBid.Pips);

            _db.Bids.Add(new Bid
            {
                PlayerId = game.PlayerTurnId.Value,
                Call = "liar",
                GameId = game.Id
            });
            _db.SaveChanges();

            await DisplayAllDice(game);


            if (countOfPips >= previousBid.Quantity)
            {
                await ReplyAsync($"There was in fact {countOfPips} dice.");
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, GetCurrentPlayer(game));
            }
            else
            {
                await ReplyAsync($"There was actually {countOfPips} dice.");
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.Player);
            }

            await RollDice(game);
        }

        private async Task HandlePipBid(string[] bidText, Data.Game game, Player biddingPlayer)
        {
            int quantity;
            int pips;
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
            if (pips < 2 || pips > 6) return;

            var bid = new Bid
            {
                Call = "",
                Pips = pips,
                Quantity = quantity,
                PlayerId = biddingPlayer.Id,
                GameId = game.Id
            };

            if (await VerifyBid(bid) == false) return;

            _db.Bids.Add(bid);
            _db.SaveChanges();

            SetNextPlayer(game, biddingPlayer);

            var nextPlayer = GetCurrentPlayer(game);

            await ReplyAsync($"{biddingPlayer.Username} bids {quantity} {pips}s. {nextPlayer.Username} is next");
        }

        private Player GetBiddingPlayer(Data.Game game)
        {
            return _db.Players
                .AsQueryable()
                .Single(x => x.Id == game.PlayerTurnId);
        }


        private async Task DisplayAllDice(Data.Game game)
        {
            var players = (await GetPlayersAsync(game)).Where(x => x.NumberOfDice > 0);
            var allDice = players.Select(x => $"{x.Username}'s Dice: {x.Die1} {x.Die2} {x.Die3} {x.Die4} {x.Die5}".TrimEnd());
            await ReplyAsync(string.Join("\n", allDice));
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Data.Game game, Player currentPlayer)
        {
            currentPlayer.NumberOfDice -= 1;

            await ReplyAsync($"{currentPlayer.Username} loses a die and is down to {currentPlayer.NumberOfDice} dice.");

            if (currentPlayer.NumberOfDice == 0)
            { 
                SetNextPlayer(game, currentPlayer);
            }
            else
            {
                game.PlayerTurnId = currentPlayer.Id;
                _db.SaveChanges();
            }
        }

        private int GetNumberOfDiceMatchingBid(Data.Game game, int pips)
        {
            var players = _db.Players.AsQueryable().Where(x => x.GameId == game.Id).ToList();

            var countOfPips = players.Sum(x =>
            {
                var d1 = x.Die1 == pips || x.Die1 == 1 ? 1 : 0;
                var d2 = x.Die2 == pips || x.Die2 == 1 ? 1 : 0;
                var d3 = x.Die3 == pips || x.Die3 == 1 ? 1 : 0;
                var d4 = x.Die4 == pips || x.Die4 == 1 ? 1 : 0;
                var d5 = x.Die5 == pips || x.Die5 == 1 ? 1 : 0;
                return d1 + d2 + d3 + d4 + d5;
            });
            return countOfPips;
        }

        private Player GetCurrentPlayer(Data.Game game)
        {
            return _db.Players.AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.Id == game.PlayerTurnId).Single();
        }

        private void SetNextPlayer(Data.Game game, Player currentPlayer)
        {
            var playerIds = _db.Players
                .AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.NumberOfDice > 0)
                .Select(x => x.Id)
                .ToList();

            var playerIndex = playerIds.FindIndex(x => x == currentPlayer.Id);

            if (playerIndex >= playerIds.Count - 1)
            {
                game.PlayerTurnId = playerIds.ElementAt(0);
            }
            else
            {
                game.PlayerTurnId = playerIds.ElementAt(playerIndex + 1);
            }

            _db.SaveChanges();
        }

        private async Task<bool> VerifyBid(Bid bid)
        {
            var game = await GetGameAsync(IN_PROGRESS);
            Bid mostRecentBid = GetMostRecentBid(game);
            if (mostRecentBid == null) return true;

            if (bid.Quantity < mostRecentBid.Quantity)
            {
                await ReplyAsync("Bid has to be higher.");
                return false;
            }
            if (bid.Quantity == mostRecentBid.Quantity && bid.Pips <= mostRecentBid.Pips)
            {
                await ReplyAsync("Bid has to be higher.");
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

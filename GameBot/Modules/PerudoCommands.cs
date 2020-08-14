using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

            await ReplyAsync($"New game created.");
            await ReplyAsync("Add players with \"!add @user\". Start the game with \"!start\".");
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

            // add check for adding same player twice.
            var players = await GetPlayersAsync(game);
            if (players.FirstOrDefault(x => x.Username == userToAdd.Username) != null)
            {
                await ReplyAsync($"{GetUserNickname(userToAdd.Username)} is already in the game.");
                return;
            }

            _db.Players.Add(new Player
            {
                GameId = game.Id,
                Username = userToAdd.Username
            });

            _db.SaveChanges();

            await ReplyAsync($"{GetUserNickname(userToAdd.Username)} added to game.");
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
            if (userToRemove == null) return;

            _db.Players.Remove(userToRemove);
            _db.SaveChanges();

            await ReplyAsync($"{GetUserNickname(userToAdd.Username)} removed from game.");
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

            await ReplyAsync($"Starting the game!");
            await ReplyAsync($"Use \"!bid 2 2s\" or \"!exact\" or \"!liar\" to play.");

            await RollDice(game);

            _db.SaveChanges();

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
                await ReplyAsync($"{GetUser(activePlayers.Single().Username).Mention} is the winner with {activePlayers.Single().NumberOfDice} dice remaining!");

                game.State = ENDED;
                _db.SaveChanges();
                return;
            }

            var r = new Random();

            var players2 = _db.Players
                .AsQueryable()
                .Where(x => x.GameId == game.Id)
                .Where(x => x.NumberOfDice > 0)
                .ToList();

            var botKeys = _db.BotKeys.AsQueryable()
                .ToList();

            foreach (var player in players2)
            {
                List<int?> dice = new List<int?>();
                for (int i = 0; i < player.NumberOfDice; i++)
                {
                    dice.Add(r.Next(1, 7));
                }

                dice.Sort();

                player.Die1 = dice.ElementAtOrDefault(0);
                player.Die2 = dice.ElementAtOrDefault(1);
                player.Die3 = dice.ElementAtOrDefault(2);
                player.Die4 = dice.ElementAtOrDefault(3);
                player.Die5 = dice.ElementAtOrDefault(4);

                var user = Context.Guild.Users.Single(x => x.Username == player.Username);
                var message = $"Your dice: {player.Die1.GetEmoji()} {player.Die2.GetEmoji()} {player.Die3.GetEmoji()} {player.Die4.GetEmoji()} {player.Die5.GetEmoji()}";

                var botKey = botKeys.FirstOrDefault(x => x.Username == player.Username);

                if (botKey == null)
                {
                    try
                    {
                        await user.SendMessageAsync(message);
                    }
                    catch (Exception e)
                    {
                        await SendEncryptedDice(player, user, player.Username);
                    }
                } else
                {
                    await SendEncryptedDice(player, user, botKey.BotAesKey);
                }
            }
            _db.SaveChanges();

            await DisplayCurrentStandings(game);

            await base.ReplyAsync($"A new round has begun. {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.");
        }

        private async Task SendEncryptedDice(Player player, SocketGuildUser user, string botKey)
        {
            var diceText = $"{player.Die1} {player.Die2} {player.Die3} {player.Die4} {player.Die5}";
            var encoded = SimpleAES.AES256.Encrypt(diceText, botKey);
            await ReplyAsync($"{user.Mention}'s dice: ||{encoded}||");
        }

        private async Task DisplayCurrentStandings(Data.Game game)
        {
            var players = (await GetPlayersAsync(game)).Where(x => x.NumberOfDice > 0);
            var totalDice = players.Sum(x => x.NumberOfDice);

            //await ReplyAsync($".\nCurrent standings:\n{string.Join("\n", playerDiceLeft)}\nTotal Dice: {totalDice}.\n.");

            var playerList = string.Join("\n", players.Select(x => $"`{x.NumberOfDice}` {GetUserNickname(x.Username)}"));

            var builder = new EmbedBuilder()
                .WithTitle("Current standings")
                .AddField("Users", $"{playerList}\n\nTotal dice left: `{totalDice}`", inline:false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

        private SocketGuildUser GetUser(string username)
        {
            return Context.Guild.Users.First(x => x.Username == username);
        }

        private string GetUserNickname(string username)
        {
            var nickname = GetUser(username).Nickname;
            if (string.IsNullOrEmpty(nickname)) return username;
            return nickname;
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

            var biddingPlayer = GetCurrentPlayer(game);

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

            await DisplayAllDice(game);

            await ReplyAsync($"{GetUserNickname(biddingPlayer.Username)} called **exact** on `{previousBid.Quantity}` ˣ {previousBid.Pips.GetEmoji()}.");


            if (countOfPips == previousBid.Quantity)
            {
                await ReplyAsync("The madman did it! It was exact!");
            }
            else
            {
                await ReplyAsync($"There was actually `{countOfPips}` dice. {GetUser(biddingPlayer.Username).Mention} loses a die.");
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer);
            }

            await RollDice(game);
        }

        [Command("liar")]
        public async Task Liar(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = await GetGameAsync(IN_PROGRESS);

            var biddingPlayer = GetCurrentPlayer(game);

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

            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch
            {
            }

            await DisplayAllDice(game);

            await ReplyAsync($"{GetUserNickname(biddingPlayer.Username)} called **liar** on `{previousBid.Quantity}` ˣ {previousBid.Pips.GetEmoji()}.");

            if (countOfPips >= previousBid.Quantity)
            {
                await ReplyAsync($"There was actually `{countOfPips}` dice. {GetUser(biddingPlayer.Username).Mention} loses a die.");
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, GetCurrentPlayer(game));
            }
            else
            {
                await ReplyAsync($"There was actually `{countOfPips}` dice. {GetUser(previousBid.Player.Username).Mention} loses a die.");
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

            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch 
            {
            }

            await ReplyAsync($"{GetUserNickname(biddingPlayer.Username)} bids `{quantity}` ˣ {pips.GetEmoji()}. {GetUser(nextPlayer.Username).Mention} is up.");
        }

        private async Task DisplayAllDice(Data.Game game)
        {
            var players = (await GetPlayersAsync(game)).Where(x => x.NumberOfDice > 0);
            var allDice = players.Select(x => $"{GetUserNickname(x.Username)}: {x.Die1.GetEmoji()} {x.Die2.GetEmoji()} {x.Die3.GetEmoji()} {x.Die4.GetEmoji()} {x.Die5.GetEmoji()}".TrimEnd());
            await ReplyAsync($"Dice: {string.Join(",  ", allDice)}");
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Data.Game game, Player currentPlayer)
        {
            currentPlayer.NumberOfDice -= 1;

            if (currentPlayer.NumberOfDice == 0)
            {
                await ReplyAsync($"{GetUserNickname(currentPlayer.Username)} defeated.");
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
            var players = _db.Players.AsQueryable().Where(x => x.GameId == game.Id).Where(x => x.NumberOfDice > 0).ToList();

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
            return _db.Players
                .AsQueryable()
                .Single(x => x.Id == game.PlayerTurnId);
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

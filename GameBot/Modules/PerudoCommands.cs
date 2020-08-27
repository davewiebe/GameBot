using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int SETUP = 0;
        private const int IN_PROGRESS = 1;
        private const int ENDED = 2;

        [Command("test")]
        public async Task Test()
        {
            if (_botType != "perudo") return;
            await SendMessage("Test1");
            await SendMessage("Test2");
            await SendMessage("Test3");
            await SendMessage("Test4");
            await SendMessage("Test5");
            await SendMessage("Test6");
            await SendMessage("Test7");
            await SendMessage("Test8");
            await SendMessage("Test9");
            await SendMessage("Test10");
            await SendMessage("Test11");
            await SendMessage("Test12");
            await SendMessage("Test13");
            await SendMessage("Test14");
            await SendMessage("Test15");
            await SendMessage("Test16");
            await SendMessage("Test17");
            await SendMessage("Test18");
            await SendMessage("Test19");
            await SendMessage("Test20");
        }


            [Command("new")]
        public async Task NewGame()
        {
            if (_botType != "perudo") return;

            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .SingleOrDefault(x => x.State == IN_PROGRESS || x.State == SETUP) != null)
            {
                string message = $"A game is already in progress.";
                await SendMessage(message);
                return;
            }

            _db.Games.Add(new Data.Game
            {
                ChannelId = Context.Channel.Id,
                State = 0,
                NumberOfDice = 5,
                Penalty = 1,
                RandomizeBetweenRounds = false,
                WildsEnabled = false
            }) ;
            _db.SaveChanges();

            await SendMessage($"New game created. Use the following commands:\n" +
                $"`!add @user` to add players.\n" +
                $"`!remove @user` to remove players.\n" +
                $"`!option dice 5` to set the number of dice.\n" +
                $"`!option penalty 1` to set the number of dice lost for an incorrect bid.\n" +
                $"`!option randomize` to toggle randomizing player order between rounds.\n" +
                $"`!option wild` to toggle bidding on wilds.\n" +
                $"`!start` to start the game.");
        }

        [Command("game")]
        public async Task Game(params string[] bidText)
        {
            var game = GetGame(SETUP);
            if (game != null)
            {
                var randomizeText = "";
                if (game.RandomizeBetweenRounds) randomizeText = "Player order will be randomized between rounds";
                else randomizeText = "Player order will not be randomized between rounds";

                var wildsText = "";
                if (game.WildsEnabled) wildsText = $"Players can bid on wild dice";
                else wildsText = "Players cannot bid on wild dice";

                var players = GetPlayers(game);
                await SendMessage($"*A game is being setup*\n" +
                    $"**Players**\n" +
                    $"{string.Join("\n", players.Select(x => GetUserNickname(x.Username)))}\n" +
                    $"\n" +
                    $"**Options**\n" +
                    $"Each player starts with {game.NumberOfDice} dice\n" +
                    $"The penalty for an incorrect call is {game.Penalty} dice\n" +
                    $"{randomizeText}\n" +
                    $"{wildsText}\n");
                return;
            }

            game = GetGame(IN_PROGRESS);
            if (game != null)
            {
                var nextPlayer = GetCurrentPlayer(game);
                var bid = GetMostRecentBid(game);
                var recentBidText = "";
                if (bid != null)
                {
                    recentBidText = $"The most recent bid was for `{ bid.Quantity}` ˣ { bid.Pips.GetEmoji()}\n";
                }
                await DisplayCurrentStandings(game);
                await SendMessage($"{recentBidText}It's {GetUserNickname(nextPlayer.Username)}'s turn.");
                return;
            }
            await SendMessage("There are no games in progress.");
        }
        private async Task SendMessage(string message)
        {
            var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
            await base.ReplyAsync(message, options: requestOptions);
        }

        [Command("option")]
        public async Task Options(params string[] stringArray)
        {
            if (_botType != "perudo") return;

            if (stringArray[0] == "dice")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                if (numberOfDice > 0 && numberOfDice <= 100)
                {
                    var game = GetGame(SETUP);

                    game.NumberOfDice = numberOfDice;
                    _db.SaveChanges();

                    await SendMessage($"Each player starts with {numberOfDice} dice");
                }
                await Options(stringArray.Skip(2).ToArray());
            }

            if (stringArray[0] == "penalty")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                if (numberOfDice > 0 && numberOfDice <= 100)
                {
                    var game = GetGame(SETUP);

                    game.Penalty = numberOfDice;
                    _db.SaveChanges();

                    await SendMessage($"The penalty for an incorrect call is {numberOfDice} dice");
                }
                await Options(stringArray.Skip(2).ToArray());
            }

            if (stringArray[0] == "randomize")
            {
                var game = GetGame(SETUP);

                game.RandomizeBetweenRounds = !game.RandomizeBetweenRounds;
                _db.SaveChanges();
                if (game.RandomizeBetweenRounds) await SendMessage($"Player order will be randomized between rounds.");
                else await SendMessage("Player order will not be randomized between rounds.");

                await Options(stringArray.Skip(1).ToArray());
            }

            if (stringArray[0] == "wild")
            {
                var game = GetGame(SETUP);

                game.WildsEnabled = !game.WildsEnabled;
                _db.SaveChanges();
                if (game.WildsEnabled) await SendMessage($"Players can bid on wild dice.");
                else await SendMessage("Players cannot bid on wild dice.");

                await Options(stringArray.Skip(1).ToArray());
            }
        }

        [Command("add")]
        public async Task AddUserToGame(string user)
        {
            if (_botType != "perudo") return;

            var userToAdd = Context.Message.MentionedUsers.First();

            var game = GetGame(SETUP);
            if (game == null)
            {
                await SendMessage($"Unable to add players at this time.");
                return;
            }

            // add check for adding same player twice.
            bool userAlreadyExistsInGame = UserAlreadyExistsInGame(userToAdd, game);
            if (userAlreadyExistsInGame)
            {
                await SendMessage($"{GetUserNickname(userToAdd.Username)} is already in the game.");
                return;
            }

            _db.Players.Add(new Player
            {
                GameId = game.Id,
                Username = userToAdd.Username,
                IsBot = userToAdd.IsBot
            });

            _db.SaveChanges();

            await SendMessage($"{GetUserNickname(userToAdd.Username)} added to game.");
        }

        private bool UserAlreadyExistsInGame(SocketUser userToAdd, Data.Game game)
        {
            var players = GetPlayers(game);
            bool userAlreadyExistsInGame = players.FirstOrDefault(x => x.Username == userToAdd.Username) != null;
            return userAlreadyExistsInGame;
        }

        [Command("remove")]
        public async Task RemoveUserFromGame(string user)
        {
            if (_botType != "perudo") return;

            var userToAdd = Context.Message.MentionedUsers.First();

            var game = GetGame(SETUP);
            if (game == null)
            {
                await SendMessage($"Unable to remove players at this time.");
                return;
            }

            var userToRemove = _db.Players.FirstOrDefault(x => x.GameId == game.Id && x.Username == userToAdd.Username);
            if (userToRemove == null) return;

            _db.Players.Remove(userToRemove);
            _db.SaveChanges();

            await SendMessage($"{GetUserNickname(userToAdd.Username)} removed from game.");
        }

        private async Task<bool> ValidateState(int stateId)
        {
            if (_botType != "perudo") return false;
            var game = GetGame(stateId);

            if (game == null)
            {
                await SendMessage($"Cannot do that at this time.");
                return false;
            }
            return true;
        }

        private Data.Game GetGame(int stateId)
        {
            return _db.Games.AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .SingleOrDefault(x => x.State == stateId);
        }

        [Command("start")]
        public async Task Start()
        {
            if (await ValidateState(SETUP) == false) return;

            var game = GetGame(SETUP);

            ShufflePlayers(game);
            SetDice(game);

            var players = GetPlayers(game);

            game.State = IN_PROGRESS;
            game.PlayerTurnId = players.First().Id;

            await SendMessage($"Starting the game!\nUse \"!bid 2 2s\" or \"!exact\" or \"!liar\" to play.");

            await RollDice(game);

            _db.SaveChanges();

        }

        private void ShufflePlayers(Data.Game game)
        {
            var players = GetPlayers(game);
            var r = new Random();
            var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();

            var turnOrder = 0;
            foreach (var player in shuffledPlayers)
            {
                player.TurnOrder = turnOrder;
                turnOrder += 1;
            }
            _db.SaveChanges();
        }

        private void SetDice(Data.Game game)
        {
            var players = GetPlayers(game);

            foreach (var player in players)
            {
                player.NumberOfDice = game.NumberOfDice;
            }
            _db.SaveChanges();
        }

        private List<Player> GetPlayers(Data.Game game)
        {
            return _db.Players.AsQueryable().Where(x => x.GameId == game.Id).OrderBy(x => x.TurnOrder).ToList();
        }

        private async Task RollDice(Data.Game game)
        {
            // IF THERE IS ONLY ONE PLAYER LEFT, ANNOUNCE THAT THEY WIN
            var players = GetPlayers(game);

            var activePlayers = players.Where(x => x.NumberOfDice > 0);
            bool onlyOnePlayerLeft = activePlayers.Count() == 1;
            if (onlyOnePlayerLeft)
            {
                await SendMessage($":trophy: {GetUser(activePlayers.Single().Username).Mention} is the winner with `{activePlayers.Single().NumberOfDice}` dice remaining! :trophy:");

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
                List<int> dice = new List<int>();
                for (int i = 0; i < player.NumberOfDice; i++)
                {
                    dice.Add(r.Next(1, 7));
                }

                dice.Sort();

                player.Dice = string.Join(",", dice);

                var user = Context.Guild.Users.Single(x => x.Username == player.Username);
                var message = $"Your dice: {string.Join(" ", dice.Select(x => x.GetEmoji()))}";

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

            if (game.RandomizeBetweenRounds) ShufflePlayers(game);

            await DisplayCurrentStandingsForBots(game);
            await DisplayCurrentStandings(game);

            await SendMessage($"A new round has begun. {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.");
        }

        private async Task SendEncryptedDice(Player player, SocketGuildUser user, string botKey)
        {
            var diceText = $"{player.Dice.Replace(",", " ")}";
            var encoded = SimpleAES.AES256.Encrypt(diceText, botKey);
            await SendMessage($"{user.Mention}'s dice: ||{encoded}||");
        }

        private async Task DisplayCurrentStandings(Data.Game game)
        {
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0);
            var totalDice = players.Sum(x => x.NumberOfDice);

            var playerList = string.Join("\n", players.Select(x => $"`{x.NumberOfDice}` {GetUserNickname(x.Username)}"));

            var builder = new EmbedBuilder()
                .WithTitle("Current standings")
                .AddField("Users", $"{playerList}\n\nTotal dice left: `{totalDice}`", inline:false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
        private async Task DisplayCurrentStandingsForBots(Data.Game game)
        {
            var players = GetPlayers(game);
            if (!players.Any(x => x.IsBot)) return;

            players = players.Where(x => x.NumberOfDice > 0).ToList();
            var totalDice = players.Sum(x => x.NumberOfDice);

            var monkey = new
            {
                Players = players.Select(x => new { Username = GetUserNickname(x.Username), DiceCount = x.NumberOfDice }),
                TotalPlayers = players.Count(),
                TotalDice = totalDice
            };

            await SendMessage($"Current standings for bots: ||{JsonConvert.SerializeObject(monkey)}||");
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

            var game = GetGame(IN_PROGRESS);
            if (game != null) game.State = ENDED;

            game = GetGame(SETUP);
            if (game != null) game.State = ENDED;

            _db.SaveChanges();

            await SendMessage("I'll be back.");
        }


        [Command("bid")]
        public async Task Bid(params string[] bidText)
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);

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

            var game = GetGame(IN_PROGRESS);

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

            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **exact** on `{previousBid.Quantity}` ˣ {previousBid.Pips.GetEmoji()}.");

            Thread.Sleep(4000);

            if (countOfPips == previousBid.Quantity)
            {
                Thread.Sleep(3000);
                await SendMessage(":zany_face: The madman did it! It was exact! :zany_face:");

                await SendDiceAsStringForBots(game);
                await GetRoundSummary(game);
            }
            else
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(biddingPlayer.Username).Mention} loses {game.Penalty} dice. :fire:");
                await SendDiceAsStringForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer);
            }

            Thread.Sleep(4000);
            await RollDice(game);
        }

        [Command("liar")]
        public async Task Liar(params string[] bidText)
        {
            if (bidText.Length != 0) return;

            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);

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

            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **liar** on `{previousBid.Quantity}` ˣ {previousBid.Pips.GetEmoji()}.");

            Thread.Sleep(4000);

            if (countOfPips >= previousBid.Quantity)
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(biddingPlayer.Username).Mention} loses {game.Penalty} dice. :fire:");

                await SendDiceAsStringForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, GetCurrentPlayer(game));
            }
            else
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(previousBid.Player.Username).Mention} loses {game.Penalty} dice. :fire:");

                await SendDiceAsStringForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.Player);
            }

            Thread.Sleep(4000);
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
            if (pips < 1 || pips > 6) return;


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
            var bidderNickname = GetUserNickname(biddingPlayer.Username);
            var nextPlayerNickname = GetUserNickname(nextPlayer.Username);
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
            } else
            {
                await SendMessage(userMessage);
            }
        }

        private bool AreBotsInGame(Data.Game game)
        {
            var players = GetPlayers(game);
            return players.Any(x => x.IsBot);
        }

        private async Task SendDiceAsStringForBots(Data.Game game)
        {
            var players = GetPlayers(game);
            if (!players.Any(x => x.IsBot)) return;

            var playerDice = players.Where(x => x.NumberOfDice > 0).ToList()
                .Select(x => new { Username = GetUserNickname(x.Username), 
                    Dice = x.Dice });

            await SendMessage($"Round summary for bots: ||{JsonConvert.SerializeObject(playerDice)}||");
        }

        private async Task GetRoundSummary(Data.Game game)
        {
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0).ToList();
            var playerDice = players.Select(x => $"{GetUserNickname(x.Username)}: {string.Join(" ", x.Dice.Split(",").Select(x => int.Parse(x).GetEmoji()))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            var allDiceGrouped = allDice
                .GroupBy(x => x)
                .OrderBy(x => x.Key);

            var countOfOnes = allDiceGrouped.SingleOrDefault(x => x.Key == 1)?.Count();

            var listOfAllDiceCounts = allDiceGrouped.Select(x => $"`{x.Count()}` ˣ {x.Key.GetEmoji()}");

            List<string> monkey = new List<string>();
            for (int i = 2; i <= 6; i++)
            {
                var countOfX = allDiceGrouped.SingleOrDefault(x => x.Key == i)?.Count();
                var count1 = countOfOnes ?? 0;
                var countX = countOfX ?? 0;
                monkey.Add($"`{count1 + countX }` ˣ {i.GetEmoji()}");
            }

            var builder = new EmbedBuilder()
                .WithTitle("Round Summary")
                .AddField("Users", $"{string.Join("\n", playerDice)}", inline: true)
                .AddField("Dice", $"{string.Join("\n", listOfAllDiceCounts)}", inline: true)
                .AddField("Totals", $"{string.Join("\n", monkey)}", inline: true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }



        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Data.Game game, Player currentPlayer)
        {
            currentPlayer.NumberOfDice -= game.Penalty;

            if (currentPlayer.NumberOfDice < 0) currentPlayer.NumberOfDice = 0;

            if (currentPlayer.NumberOfDice <= 0)
            {
                await SendMessage($":fire::skull::fire: {GetUserNickname(currentPlayer.Username)} defeated :fire::skull::fire:");
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
            var players = GetPlayers(game).Where(x => x.NumberOfDice > 0).ToList();

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            return allDice.Count(x => x == pips || x == 1);
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
                .OrderBy(x => x.TurnOrder)
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
            var game = GetGame(IN_PROGRESS);
            Bid mostRecentBid = GetMostRecentBid(game);

            if (game.WildsEnabled == false && bid.Pips == 1)
            {
                await SendMessage("Cannot bid on wilds this game.");
                return false;
            }

            if (mostRecentBid == null) {
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
                var hasGoneToOnesAlready = _db.Bids.AsQueryable().Where(x => x.GameId == game.Id).Where(x => x.Pips == 1).Any();
                if (hasGoneToOnesAlready)
                {
                    await SendMessage("Cannot switch to wilds more than once a round.");
                    return false;
                }


                if (bid.Quantity*2 <= mostRecentBid.Quantity)
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

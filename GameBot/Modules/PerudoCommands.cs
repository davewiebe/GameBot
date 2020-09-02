using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int SETUP = 0;
        private const int IN_PROGRESS = 1;
        private const int ENDED = 2;

        [Command("error")]
        public async Task Error()
        {
            throw new Exception("Test error. Do not panic!");
        }

        [Command("new")]
        public async Task NewGame(params string[] stringArray)
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
                WildsEnabled = true,
                ExactCallBonus = 0,
                ExactCallPenalty = 0,
                CanCallExactAnytime = false,
                CanCallLiarAnytime = false,
                CanBidAnytime = false
            });
            _db.SaveChanges();


            var commands = 
                $"`!add @user` to add players.\n" +
                $"`!remove @user` to remove players.\n" +
                $"`!option xyz` to set round options.\n" +
                $"`!status` to view current status.\n" +
                $"`!start` to start the game.";


            var builder = new EmbedBuilder()
                            .WithTitle("New game created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);


            var game = GetGame(SETUP);
            AddUsers(game, Context.Message);
            SetOptions(stringArray);

            await Status();
        }

        public List<string> GetOptions(Data.Game game)
        {
            var options = new List<string>();

            options.Add($"Each player starts with `{game.NumberOfDice}` dice");
            options.Add($"The penalty for an incorrect call is `{game.Penalty}` dice");

            if (game.RandomizeBetweenRounds) options.Add("Player order will be **randomized** between rounds");
            if (game.WildsEnabled) options.Add("Players can bid on **wild** dice.");
            if (game.ExactCallBonus > 0) options.Add($"Players that call **exact** with more than 2 players in the game will win `{game.ExactCallBonus}` dice.");
            if (game.ExactCallPenalty > 0) options.Add($"Players that call **exact** with more than 2 players in the game will cause everyone else to lose `{game.ExactCallPenalty}` dice.");
            if (game.CanCallLiarAnytime) options.Add("Players can call **liar** out of turn.");
            if (game.CanCallExactAnytime) options.Add("Players can call **exact** out of turn.");
            if (game.CanBidAnytime) options.Add("Players can **bid** out of turn.");

            return options;
        }

        [Command("status")]
        public async Task Status()
        {
            var game = GetGame(SETUP);
            if (game != null)
            {
                var players = GetPlayers(game);
                var options = GetOptions(game);

                var builder = new EmbedBuilder()
                                .WithTitle($"Game set up")
                                .AddField($"Players ({players.Count()})", $"{string.Join("\n", players.Select(x => GetUserNickname(x.Username)))}", inline: false)
                                .AddField("Options", $"{string.Join("\n", options)}", inline: false);
                var embed = builder.Build();

                await Context.Channel.SendMessageAsync(
                    embed: embed)
                    .ConfigureAwait(false);
                return;
            }

            game = GetGame(IN_PROGRESS);
            if (game != null)
            {
                var nextPlayer = GetCurrentPlayer(game);
                var bid = GetMostRecentBid(game);
                await DisplayCurrentStandings(game);


                var options = GetOptions(game);
                var builder = new EmbedBuilder()
                                .WithTitle("Game options")
                                .AddField("Options", $"{string.Join("\n", options)}", inline: false);
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);


                var recentBidText = "";
                if (bid != null)
                {
                    recentBidText = $"The most recent bid was for `{ bid.Quantity}` ˣ { bid.Pips.GetEmoji()}\n";
                }
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

        [Command("options")]
        public async Task Options(params string[] stringArray)
        {
            await Option(stringArray);
        }

        [Command("option")]
        public async Task Option(params string[] stringArray)
        {
            if (_botType != "perudo") return;

            if (stringArray.Length == 0)
            {
                var options =
                $"`!option dice x` to start the game with `x` dice\n" +
                $"`!option penalty x` to set the penalty for an incorrect bid/call to `x`\n" +
                $"`!option randomize` to toggle **randomizing** player order\n" +
                $"`!option nowild` to disable bidding on **wilds**\n" +
                $"`!option exact x y` a correct exact bid wins the caller `x` dice and/or causes other players to lose `y` dice\n" +
                $"`!option exactanytime` to toggle allowing **exact** calls at any time\n" +
                $"`!option liaranytime` to toggle allowing **liar** calls at any time\n" +
                $"`!option bidanytime` to toggle allowing **bids** at any time";


                var builder = new EmbedBuilder()
                                .WithTitle("Game options")
                                .AddField("Commands", options, inline: false);
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                return;
            }

            SetOptions(stringArray);

            await Status();
        }

        private void SetOptions(string[] stringArray)
        {
            if (stringArray.Length == 0) return;
            if (stringArray[0] == "dice")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                if (numberOfDice > 0 && numberOfDice <= 100)
                {
                    var game = GetGame(SETUP);

                    game.NumberOfDice = numberOfDice;
                    _db.SaveChanges();

                    //await SendMessage($"Each player starts with `{numberOfDice}` dice");
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }

            else if (stringArray[0] == "penalty")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                if (numberOfDice > 0 && numberOfDice <= 100)
                {
                    var game = GetGame(SETUP);

                    game.Penalty = numberOfDice;
                    _db.SaveChanges();

                    //await SendMessage($"The penalty for an incorrect call is `{numberOfDice}` dice");
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }

            else if (stringArray[0] == "randomize")
            {
                var game = GetGame(SETUP);

                game.RandomizeBetweenRounds = !game.RandomizeBetweenRounds;
                _db.SaveChanges();
                //if (game.RandomizeBetweenRounds) await SendMessage($"Player order will be **randomized** between rounds.");
                //else await SendMessage("Player order will not be **randomized** between rounds.");

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "nowild")
            {
                var game = GetGame(SETUP);

                game.WildsEnabled = !game.WildsEnabled;
                _db.SaveChanges();
                //if (game.WildsEnabled) await SendMessage($"Players can bid on **wild** dice.");
                //else await SendMessage("Players cannot bid on **wild** dice.");

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "bidanytime")
            {
                var game = GetGame(SETUP);

                game.CanBidAnytime = !game.CanBidAnytime;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exactanytime")
            {
                var game = GetGame(SETUP);

                game.CanCallExactAnytime = !game.CanCallExactAnytime;
                _db.SaveChanges();
                //if (game.CanCallExactAnytime) await SendMessage($"Players can call **exact** out of turn.");
                //else await SendMessage("Players cannot call **exact** out of turn.");

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "liaranytime")
            {
                var game = GetGame(SETUP);

                game.CanCallLiarAnytime = !game.CanCallLiarAnytime;
                _db.SaveChanges();
                //if (game.CanCallLiarAnytime) await SendMessage($"Players can call **liar** out of turn.");
                //else await SendMessage("Players cannot call **liar** out of turn.");

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "exact")
            {
                var game = GetGame(SETUP);

                var exactCallerBonus = int.Parse(stringArray[1]);
                var exactOthersPenalty = int.Parse(stringArray[2]);

                if (exactCallerBonus >= 0 && exactCallerBonus <= 100 && exactOthersPenalty >= 0 && exactOthersPenalty <= 100)
                {

                    game.ExactCallBonus = exactCallerBonus;
                    game.ExactCallPenalty = exactOthersPenalty;
                    _db.SaveChanges();

                    var summary = "";

                    //if (game.ExactCallBonus > 0) summary += $"Players that call **exact** with more than 2 players in the game will win `{exactCallerBonus}` dice.";
                    //if (game.ExactCallPenalty > 0) summary += $"\nPlayers that call **exact** with more than 2 players in the game will cause everyone else to lose `{exactOthersPenalty}` dice.";
                    //if (summary == "") summary = $"Calling **exact** resets the round only.";

                    //await SendMessage(summary);
                }

                SetOptions(stringArray.Skip(3).ToArray());
            }
            else
            {
                SetOptions(stringArray.Skip(1).ToArray());
            }
        }

        [Command("add")]
        public async Task AddUserToGame(params string[] stringArray)
        {
            if (_botType != "perudo") return;

            var game = GetGame(SETUP);
            if (game == null)
            {
                await SendMessage($"Unable to add players at this time.");
                return;
            }

            AddUsers(game, Context.Message);

            await Status();
        }

        private void AddUsers(Data.Game game, SocketUserMessage message)
        {
            foreach (var userToAdd in message.MentionedUsers)
            {
                // add check for adding same player twice.
                bool userAlreadyExistsInGame = UserAlreadyExistsInGame(userToAdd, game);
                if (userAlreadyExistsInGame)
                {
                    //await SendMessage($"{GetUserNickname(userToAdd.Username)} is already in the game.");
                    continue;
                }

                _db.Players.Add(new Player
                {
                    GameId = game.Id,
                    Username = userToAdd.Username,
                    IsBot = userToAdd.IsBot
                });

                _db.SaveChanges();

                //await SendMessage($"{GetUserNickname(userToAdd.Username)} added to game.");
            }
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
            game.RoundStartPlayerId = players.First().Id;

            await SendMessage($"Starting the game!\nUse `!bid 2 2s` or `!exact` or `!liar` to play.");

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
            _db.SaveChanges();
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

            _db.SaveChanges();
            game.RoundStartPlayerId = GetCurrentPlayer(game).Id;
            _db.SaveChanges();

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
                .AddField("Users", $"{playerList}\n\nTotal dice left: `{totalDice}`\nQuick maths: {totalDice}/3 = `{totalDice / 3.0:F2}`", inline: false);
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

            var currentStandings = new
            {
                Players = players.Select(x => new { Username = GetUserNickname(x.Username), DiceCount = x.NumberOfDice }),
                TotalPlayers = players.Count(),
                TotalDice = totalDice
            };

            await SendMessage($"Current standings for bots: ||{JsonConvert.SerializeObject(currentStandings)}||");
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


            if (game.CanBidAnytime)
            {
                var currentPlayer = GetPlayers(game)
                    .Where(x => x.NumberOfDice > 0)
                    .SingleOrDefault(x => x.Username == Context.User.Username);
                if (currentPlayer == null) return;
                game.PlayerTurnId = currentPlayer.Id;

                // reset turn order
                currentPlayer.TurnOrder = 0;
                var players = _db.Players.AsQueryable().Where(x => x.GameId == game.Id).OrderBy(x => x.TurnOrder).Where(x => x.NumberOfDice > 0);
                var order = 1;
                foreach (var player in players)
                {
                    player.TurnOrder = order;
                    order += 1;
                }

                _db.SaveChanges();
            }


            var biddingPlayer = GetCurrentPlayer(game);

            if (biddingPlayer.Username != Context.User.Username)
            {
                return;
            }

            await HandlePipBid(bidText, game, biddingPlayer);
        }


        [Command("exact")]
        public async Task Exact()
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);

            if (game.CanCallExactAnytime)
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

                await SendMessage($":zany_face: The madman did it! It was exact! :zany_face:");

                var numPlayersLeft = GetPlayers(game).Where(x => x.NumberOfDice > 0).Count();
                if (game.ExactCallBonus > 0 && numPlayersLeft >= 3)
                {
                    biddingPlayer.NumberOfDice += game.ExactCallBonus;
                    _db.SaveChanges();
                    await SendMessage($"\n:crossed_swords: As a bonus, they gain `{game.ExactCallBonus}` dice :crossed_swords:");
                }

                if (game.ExactCallPenalty > 0 && numPlayersLeft >= 3)
                {
                    await SendMessage($":crossed_swords: As a bonus, everyone else loses `{game.ExactCallPenalty}` dice :crossed_swords:");

                    var otherplayers = GetPlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Id != biddingPlayer.Id);
                    foreach (var player in otherplayers)
                    {
                        await DecrementDieFromPlayer(player, game.ExactCallPenalty);
                    }
                }


                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);

                SetTurnPlayerToRoundStartPlayer(game);
            }
            else
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(biddingPlayer.Username).Mention} loses {game.Penalty} dice. :fire:");
                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer);
            }

            Thread.Sleep(4000);
            await RollDice(game);
        }

        private void SetTurnPlayerToRoundStartPlayer(Data.Game game)
        {
            game.PlayerTurnId = game.RoundStartPlayerId;
            _db.SaveChanges();
        }

        [Command("liar")]
        public async Task Liar()
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);

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

            var previousBid = GetMostRecentBid(game);
            if (previousBid == null) return;
            if (previousBid.Quantity == 0) return;
            int countOfPips = GetNumberOfDiceMatchingBid(game, previousBid.Pips);

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

            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **liar** on `{previousBid.Quantity}` ˣ {previousBid.Pips.GetEmoji()}.");

            Thread.Sleep(4000);

            if (countOfPips >= previousBid.Quantity)
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(biddingPlayer.Username).Mention} loses {game.Penalty} dice. :fire:");

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer);
            }
            else
            {
                await SendMessage($"There was actually `{countOfPips}` dice. :fire: {GetUser(previousBid.Player.Username).Mention} loses {game.Penalty} dice. :fire:");

                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.Player);
            }

            Thread.Sleep(4000);
            await RollDice(game);
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

        private async Task SendRoundSummaryForBots(Data.Game game)
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

            List<string> totals = new List<string>();
            for (int i = 2; i <= 6; i++)
            {
                var countOfX = allDiceGrouped.SingleOrDefault(x => x.Key == i)?.Count();
                var count1 = countOfOnes ?? 0;
                var countX = countOfX ?? 0;
                totals.Add($"`{count1 + countX }` ˣ {i.GetEmoji()}");
            }

            var builder = new EmbedBuilder()
                .WithTitle("Round Summary")
                .AddField("Users", $"{string.Join("\n", playerDice)}", inline: true)
                .AddField("Dice", $"{string.Join("\n", listOfAllDiceCounts)}", inline: true)
                .AddField("Totals", $"{string.Join("\n", totals)}", inline: true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }


        private async Task DecrementDieFromPlayer(Player player, int penalty)
        {
            player.NumberOfDice -= penalty;

            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice <= 0)
            {
                await SendMessage($":fire::skull::fire: {GetUserNickname(player.Username)} defeated :fire::skull::fire:");
            }
            _db.SaveChanges();
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Data.Game game, Player player)
        {
            player.NumberOfDice -= game.Penalty;

            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice <= 0)
            {
                await SendMessage($":fire::skull::fire: {GetUserNickname(player.Username)} defeated :fire::skull::fire:");
                SetNextPlayer(game, player);
            }
            else
            {
                game.PlayerTurnId = player.Id;
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
                var prevRoundId = 0;
                var prevRound = _db.Bids.AsQueryable().Where(x => x.Call != "").ToList().LastOrDefault();

                if (prevRound != null) prevRoundId = prevRound.Id;

                var hasGoneToOnesAlready = _db.Bids.AsQueryable()
                    .Where(x => x.Id > prevRoundId)
                    .Where(x => x.GameId == game.Id).Where(x => x.Pips == 1).Any();
                if (hasGoneToOnesAlready)
                {
                    await SendMessage("Cannot switch to wilds more than once a round.");
                    return false;
                }


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

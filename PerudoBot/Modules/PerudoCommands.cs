using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PerudoBot.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int SETUP = 0;
        private const int IN_PROGRESS = 1;
        private const int TERMINATED = 2;
        private const int FINISHED = 3;

        [Command("error")]
        public async Task Error()
        {
            throw new Exception("Test error. Do not panic!");
        }

        [Command("rattles")]
        public async Task Rattles()
        {
            await SendMessage("Set your rattles with `!deathrattle`, `!winrattle`, and `!tauntrattle`.\nI've PM'd you your rattles.");

            var user = Context.Message.Author;

            var rattles = _db.Rattles.SingleOrDefault(x => x.Username == user.Username);
            if (rattles != null)
            {
                var message = $"deathrattle: {rattles.Deathrattle}\n" +
                    $"winrattle: {rattles.Winrattle}\n" +
                    $"tauntrattle: {rattles.Tauntrattle}";

                var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
                await user.SendMessageAsync(message, options: requestOptions);
            }

        }

        [Command("deathrattle")]
        public async Task Deathrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            // get current deathrattle
            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);

            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Deathrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            } else
            {
                currentDr.Deathrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{username}'s deathrattle updated.");
        }

        [Command("winrattle")]
        public async Task Winrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);
            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Winrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            }
            else
            {
                currentDr.Winrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{(username)}'s winrattle updated.");
        }

        [Command("tauntrattle")]
        public async Task Tauntrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);
            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Tauntrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            }
            else
            {
                currentDr.Tauntrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{(username)}'s tauntrattle updated.");
        }

        [Command("same")]
        public async Task Same(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("anotherround")]
        public async Task AnotherRound(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("again")]
        public async Task Again(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("replay")]
        public async Task Replay(params string[] stringArray)
        {
            await Redo(stringArray);
        }

        [Command("redo")]
        public async Task Redo(params string[] stringArray)
        {
            if (_db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .SingleOrDefault(x => x.State == IN_PROGRESS || x.State == SETUP) != null)
            {
                string message = $"A game is already in progress.";
                await SendMessage(message);
                return;
            }

            var lastGame = _db.Games
                .AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .OrderByDescending(x => x.Id)
                .First();


            _db.Games.Add(new Data.Game
            {
                ChannelId = Context.Channel.Id,
                State = 0,
                DateCreated = DateTime.Now,
                NumberOfDice = lastGame.NumberOfDice,
                Penalty = lastGame.Penalty,
                NextRoundIsPalifico = false,
                RandomizeBetweenRounds = lastGame.RandomizeBetweenRounds,
                WildsEnabled = lastGame.WildsEnabled,
                ExactCallBonus = lastGame.ExactCallBonus,
                ExactCallPenalty = lastGame.ExactCallPenalty,
                CanCallExactAnytime = lastGame.CanCallExactAnytime,
                CanCallLiarAnytime = lastGame.CanCallLiarAnytime,
                CanBidAnytime = lastGame.CanBidAnytime,
                Palifico = lastGame.Palifico,
                IsRanked = lastGame.IsRanked,
                GuildId = Context.Guild.Id,
                FaceoffEnabled = lastGame.FaceoffEnabled
            });
            _db.SaveChanges();


            var commands =
                $"`!add/remove @user` to add/remove players.\n" +
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
            try
            {

                SetOptions(stringArray);
            }
            catch (Exception e)
            {
                var a = "monkey";
            }

            await Status();
        }

        [Command("log")]
        public async Task Log(params string[] stringArray)
        {
            await Notes(stringArray);
        }

        [Command("note")]
        public async Task Note(params string[] stringArray)
        {
            await Notes(stringArray);
        }
        [Command("notes")]
        public async Task Notes(params string[] stringArray)
        {
            var game = GetGame(IN_PROGRESS);

            if (game == null)
            {
                var now = DateTime.Now.AddMinutes(-5);
                game = _db.Games.AsQueryable()
                    .Where(x => x.ChannelId == Context.Channel.Id)
                    .Where(x => x.State == FINISHED)
                    .Where(x => x.DateFinished > now)
                    .OrderByDescending(x => x.Id)
                    
                    .First();
            }
            var text = string.Join(" ", stringArray)
                .StripSpecialCharacters();

            if (text.Length > 256)
            {
                await SendMessage("Note is too long.");
                return;
            }
            _db.Notes.Add(new Note
            {
                Game = game,
                Username = Context.User.Username,
                Text = text
            });

            _db.SaveChanges();

            await SendMessage("Noted");
        }

        [Command("new")]
        public async Task NewGame(params string[] stringArray)
        {
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
                DateCreated = DateTime.Now,
                NumberOfDice = 5,
                Penalty = 1,
                NextRoundIsPalifico = false,
                RandomizeBetweenRounds = false,
                WildsEnabled = true,
                ExactCallBonus = 0,
                ExactCallPenalty = 0,
                CanCallExactAnytime = false,
                CanCallLiarAnytime = false,
                CanBidAnytime = false,
                Palifico = true,
                IsRanked = true,
                GuildId = Context.Guild.Id,
                FaceoffEnabled = true
            });
            _db.SaveChanges();


            var commands = 
                $"`!add/remove @user` to add/remove players.\n" +
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
            try {

                SetOptions(stringArray);
            }
            catch (Exception e)
            {
                var a = "monkey";
            }

            await Status();
        }

        public List<string> GetOptions(Data.Game game)
        {
            var options = new List<string>();

            options.Add($"Each player starts with `{game.NumberOfDice}` dice");
            if (game.Penalty == 0)
            {
                options.Add($"The penalty for an incorrect call is *the number of dice they were off by*.");
            }
            else
            {
                options.Add($"The penalty for an incorrect call is `{game.Penalty}` dice");
            }
            var exactBonuses = "";
            if (game.ExactCallBonus > 0 || game.ExactCallPenalty > 0) exactBonuses = " (exact bonuses do not apply)";

            if (game.RandomizeBetweenRounds) options.Add("Player order will be **randomized** between rounds");
            if (game.WildsEnabled) options.Add("Players can bid on **wild** dice.");
            if (game.Palifico) options.Add($"Reaching one die triggers a **Palifico** round{exactBonuses}.");
            if (game.FaceoffEnabled) options.Add("Reaching 2 dice total triggers **Faceoff** round.");
            if (game.ExactCallBonus > 0 || game.ExactCallPenalty > 0) options.Add($"Correct **exact** calls win `{game.ExactCallBonus}` dice back, and everyone else loses `{game.ExactCallPenalty}` dice, only when called out of turn (3+ players).");
            if (game.CanCallLiarAnytime) options.Add("Players can call **liar** out of turn.");
            if (game.CanCallExactAnytime) options.Add("Players can call **exact** out of turn.");
            if (game.CanBidAnytime) options.Add("Players can **bid** out of turn.");
            if (game.IsRanked) options.Add("Game is ranked and saved to highscore board.");

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
                var playersListString = string.Join("\n", players.Select(x => GetUserNickname(x.Username)));
                if (players.Count() == 0) playersListString = "none";

                var builder = new EmbedBuilder()
                                .WithTitle($"Game set up")
                                .AddField($"Players ({players.Count()})", $"{playersListString}", inline: false)
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
        private async Task SendMessage(string message, bool isTTS = false)
        {
            if (string.IsNullOrEmpty(message)) return;

            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
        }
        private async Task SendTempMessage(string message, bool isTTS = false)
        {
            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            var sentMessage = await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
            try
            {
                _ = sentMessage.DeleteAsync();
            }
            catch
            { }
        }

        [Command("scoreboard")]
        public async Task Scoreboard(params string[] stringArray)
        {
            var guildId = Context.Guild.Id;
            var players1 = _db.Players.AsQueryable()
                .Where(x => x.Game.IsRanked)
                .Where(x => x.Game.GuildId == guildId)
                .Where(x => x.Game.State == FINISHED)
                .Include(x => x.Game.Notes)
                .OrderBy(x => x.Game.DateCreated)
                .ToList();

            var players = players1
                .GroupBy(x => x.Game)
                .Where(x => x.Count() > 1);


            var i = -1;
            if (stringArray.Length == 1)
            {
                i = int.Parse(stringArray[0]);
            }

            var monk = new List<string>();
            var index = 1;
            foreach (var item in players)
            {
                if (i > -1)
                {
                    if (index != i)
                    {
                        index += 1;
                        continue;
                    }
                }
                var nonWinnerList = string.Join(", ", item.Where(x => x.Username != item.Key.Winner).Select(x => GetUserNickname(x.Username)));
                monk.Add($"`{index.ToString("D2")}. {item.Key.DateCreated:yyyy-MM-dd}` :trophy: **{GetUserNickname(item.Key.Winner)}**, {nonWinnerList}");
                index += 1;

                if (i > -1)
                {
                    monk.AddRange(item.Key.Notes.Select(x => $"**{GetUserNickname(x.Username)}**: {x.Text}"));
                } 
            }

            if (i == -1)
            {
                monk = monk.OrderByDescending(x => x).ToList();
            }

            var monkey = string.Join("\n", monk.Take(10));
            if (i == -1)
            {
                monk.Add("\nType `!leaderboard 1` to get notes on a specific game");
            }
            var builder = new EmbedBuilder()
                                .WithTitle("Leaderboard")
                                .AddField("Games", string.Join("\n", monkey), inline: false);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            return;
        }

        [Command("highscores")]
        public async Task Highscores(params string[] stringArray)
        {
            try
            {
                await Scoreboard(stringArray);

            }
            catch (Exception e)
            {
                var a = 3;
            }
        }
        [Command("leaderboard")]
        public async Task Leaderboard(params string[] stringArray)
        {
            await Scoreboard(stringArray);
        }

        [Command("highscore")]
        public async Task Highscore(params string[] stringArray)
        {
            await Scoreboard(stringArray);
        }

        [Command("options")]
        public async Task Options(params string[] stringArray)
        {
            await Option(stringArray);
        }

        [Command("option")]
        public async Task Option(params string[] stringArray)
        {
            if (stringArray.Length == 0)
            {
                var options =
                $"`!option dice x` to start the game with `x` dice\n" +
                $"`!option penalty x` to set the penalty for an incorrect bid/call to `x`\n" +
                $"`!option penalty variable` the penalty will be the difference in dice\n" +
                $"`!option randomized/ordered` to change **randomizing** player order\n" +
                $"`!option wild/nowild` to change bidding on **wilds**\n" +
                $"`!option exact x y` a correct exact bid wins the caller `x` dice and/or causes other players to lose `y` dice\n" +
                $"`!option exactanytime/noexactanytime` to change allowing **exact** calls at any time\n" +
                $"`!option liaranytime/noliaranytime` to change allowing **liar** calls at any time\n" +
                $"`!option bidanytime/nobidanytime` to change allowing **bids** at any time\n" +
                $"`!option palifico/nopalifico` to toggle **Palifico** rounds\n" +
                $"`!option faceoff/nofaceoff` to toggle **Faceoff** rounds\n" +
                $"`!option ranked/unranked` to change if a game is ranked";

                var modes =
                $"`simple` Simple rules. Good for bots\n" +
                $"`standard` The standard set of rules\n" +
                $"`chaos` Anything goes anytime\n" +
                $"`suddendeath` For a sudden death match";

                var builder = new EmbedBuilder()
                                .WithTitle("Game options")
                                .AddField("Modes", modes, inline: false)
                                .AddField("Granular options", options, inline: false);
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                return;
            }

            SetOptions(stringArray);
            try
                {

                await Status();
            }
            catch (Exception e)
            {
                var monkey = 1;
            }
        }

        private void SetOptions(string[] stringArray)
        {
            if (stringArray.Length == 0) return;

            var allText = string.Join(" ", stringArray).ToLower();
            allText = allText.Replace("simple", "nowild dice 5 penalty 1 noexactanytime noliaranytime nobidanytime exact 0 0 ordered nopalifico ranked");
            if (allText.Contains("suddendeath"))
            {
                allText = allText.Replace("suddendeath", "") + "nopalifico nofaceoff penalty 100";
            }
            allText = allText.Replace("chaos", "exactanytime liaranytime bidanytime");
            allText = allText.Replace("standard", "wild dice 5 penalty 1 noexactanytime noliaranytime nobidanytime exact 0 0 ordered palifico ranked");

            stringArray = allText.Split(" ");
            if (stringArray[0] == "dice")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                if (numberOfDice > 0 && numberOfDice <= 100)
                {
                    var game = GetGame(SETUP);

                    game.NumberOfDice = numberOfDice;
                    _db.SaveChanges();
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }

            else if (stringArray[0] == "penalty")
            {
                if (stringArray[1].ToLower() == "variable")
                {

                    var game = GetGame(SETUP);

                    game.Penalty = 0;
                    _db.SaveChanges();

                }
                else
                {
                    var numberOfDice = int.Parse(stringArray[1]);

                    if (numberOfDice > 0 && numberOfDice <= 100)
                    {
                        var game = GetGame(SETUP);

                        game.Penalty = numberOfDice;
                        _db.SaveChanges();
                    }
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }

            else if (stringArray[0] == "randomized")
            {
                var game = GetGame(SETUP);

                game.RandomizeBetweenRounds = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ordered")
            {
                var game = GetGame(SETUP);

                game.RandomizeBetweenRounds = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ranked")
            {
                var game = GetGame(SETUP);

                game.IsRanked = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "unranked")
            {
                var game = GetGame(SETUP);

                game.IsRanked = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "palifico")
            {
                var game = GetGame(SETUP);

                game.Palifico = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nopalifico")
            {
                var game = GetGame(SETUP);

                game.Palifico = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "faceoff")
            {
                var game = GetGame(SETUP);

                game.FaceoffEnabled = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nofaceoff")
            {
                var game = GetGame(SETUP);

                game.FaceoffEnabled = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nowild" || stringArray[0] == "nowilds")
            {
                var game = GetGame(SETUP);

                game.WildsEnabled = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "wild" || stringArray[0] == "wilds")
            {
                var game = GetGame(SETUP);

                game.WildsEnabled = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "bidanytime")
            {
                var game = GetGame(SETUP);

                game.CanBidAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nobidanytime")
            {
                var game = GetGame(SETUP);

                game.CanBidAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exactanytime")
            {
                var game = GetGame(SETUP);

                game.CanCallExactAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noexactanytime")
            {
                var game = GetGame(SETUP);
                game.ExactCallBonus = 0;
                game.ExactCallPenalty = 0;
                game.CanCallExactAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "liaranytime")
            {
                var game = GetGame(SETUP);

                game.CanCallLiarAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noliaranytime")
            {
                var game = GetGame(SETUP);

                game.CanCallLiarAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "exact")
            {
                var game = GetGame(SETUP);

                var exactCallerBonus = int.Parse(stringArray[1]);
                var exactOthersPenalty = int.Parse(stringArray[2]);

                if (exactCallerBonus >= 0 && exactCallerBonus <= 100 && exactOthersPenalty >= 0 && exactOthersPenalty <= 100)
                {
                    game.CanCallExactAnytime = true;
                    game.ExactCallBonus = exactCallerBonus;
                    game.ExactCallPenalty = exactOthersPenalty;
                    _db.SaveChanges();
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
            if (message.MentionedUsers.Count == 0)
            {
                AddUserToGame(game, message.Author.Username);
            }
            foreach (var userToAdd in message.MentionedUsers)
            {
                AddUserToGame(game, userToAdd.Username);
            }
        }

        private void AddUserToGame(Data.Game game, string username)
        {
            bool userAlreadyExistsInGame = UserAlreadyExistsInGame(username, game);
            if (userAlreadyExistsInGame)
            {
                return;
            }

            _db.Players.Add(new Player
            {
                GameId = game.Id,
                Username = username,
                IsBot = GetUser(username).IsBot
            });

            _db.SaveChanges();
        }

        private bool UserAlreadyExistsInGame(string username, Data.Game game)
        {
            var players = GetPlayers(game);
            bool userAlreadyExistsInGame = players.FirstOrDefault(x => x.Username == username) != null;
            return userAlreadyExistsInGame;
        }

        [Command("remove")]
        public async Task RemoveUserFromGame(string user)
        {
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
                
                var rattles = _db.Rattles.SingleOrDefault(x => x.Username == activePlayers.Single().Username);
                if (rattles != null)
                {
                    await SendMessage(rattles.Winrattle);
                }

                game.State = FINISHED;
                game.DateFinished = DateTime.Now;
                game.Winner = activePlayers.Single().Username;
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
                        var requestOptions = new RequestOptions()
                        { RetryMode = RetryMode.RetryRatelimit };
                        await user.SendMessageAsync(message, options: requestOptions);
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

            if (activePlayers.Sum(x => x.NumberOfDice) == 2 && game.FaceoffEnabled)
            {
                await SendTempMessage("!gif fight");
                await SendMessage($":face_with_monocle: Faceoff Round :face_with_monocle: {GetUser(GetCurrentPlayer(game).Username).Mention} goes first. Bid on total pips only (eg. `!bid 4`)");
            }

            else if (game.NextRoundIsPalifico)
            {
                await SendMessage($":game_die: Palifico Round :game_die: {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.\n" +
                    $"`!exact` will only reset the round - no bonuses.");
            } 
            else
            {
                await SendMessage($"A new round has begun. {GetUser(GetCurrentPlayer(game).Username).Mention} goes first.");
            }
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
            if (nickname.StripSpecialCharacters().Trim() == "") return "NULL";
            return nickname.StripSpecialCharacters();
        }

        [Command("terminate")]
        public async Task Terminate(params string[] bidText)
        {
            var game = GetGame(IN_PROGRESS);
            if (game != null) game.State = TERMINATED;

            game = GetGame(SETUP);
            if (game != null) game.State = TERMINATED;

            _db.SaveChanges();

            await SendMessage("I'll be back.");
        }

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


        [Command("exact")]
        public async Task Exact()
        {
            if (await ValidateState(IN_PROGRESS) == false) return;

            var game = GetGame(IN_PROGRESS);

            var originalBiddingPlayer = GetCurrentPlayer(game);
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

            var bidObject = previousBid.Pips.GetEmoji();
            var bidName = "dice";
            if (game.FaceoffEnabled && GetPlayers(game).Sum(x => x.NumberOfDice) == 2)
            {
                bidObject = ":record_button:";
                bidName = "pips";
            }
            await SendMessage($"{GetUserNickname(biddingPlayer.Username)} called **exact** on `{previousBid.Quantity}` ˣ {bidObject}.");

            Thread.Sleep(4000);

            if (countOfPips == previousBid.Quantity)
            {
                Thread.Sleep(3000);

                await SendMessage($":zany_face: The madman did it! It was exact! :zany_face:");

                var numPlayersLeft = GetPlayers(game).Where(x => x.NumberOfDice > 0).Count();
                if (game.ExactCallBonus > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
                {
                    biddingPlayer.NumberOfDice += game.ExactCallBonus;
                    if (biddingPlayer.NumberOfDice > game.NumberOfDice) biddingPlayer.NumberOfDice = game.NumberOfDice;
                    _db.SaveChanges();
                    await SendMessage($"\n:crossed_swords: As a bonus, they gain `{game.ExactCallBonus}` dice :crossed_swords:");
                }

                if (game.ExactCallPenalty > 0 && numPlayersLeft >= 3 && !game.NextRoundIsPalifico && originalBiddingPlayer.Id != biddingPlayer.Id)
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
                var penalty = Math.Abs(countOfPips - previousBid.Quantity);
                if (game.Penalty != 0) penalty = game.Penalty;

                await SendMessage($"There was actually `{countOfPips}` {bidName}. :fire: {GetUser(biddingPlayer.Username).Mention} loses {penalty} dice. :fire:");
                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, biddingPlayer, penalty);
            }

            Thread.Sleep(4000);
            await RollDice(game);
        }

        private void SetTurnPlayerToRoundStartPlayer(Data.Game game)
        {
            game.PlayerTurnId = game.RoundStartPlayerId;

            var thatUser = GetPlayers(game).Single(x => x.Id == game.PlayerTurnId);
            if (thatUser.NumberOfDice == 0)
            {
                SetNextPlayer(game, thatUser);
            }

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
            }
            else
            {
                var penalty = previousBid.Quantity - countOfPips;
                if (game.Penalty != 0) penalty = game.Penalty;

                await SendMessage($"There was actually `{countOfPips}` {biddingName}. :fire: {GetUser(previousBid.Player.Username).Mention} loses {penalty} dice. :fire:");


                await SendRoundSummaryForBots(game);
                await GetRoundSummary(game);
                await DecrementDieFromPlayerAndSetThierTurnAsync(game, previousBid.Player, penalty);
            }

            Thread.Sleep(4000);
            await RollDice(game);
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
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Username);
                if (deathrattle != null)
                {
                    await SendMessage(deathrattle.Deathrattle);
                }
            }

            var game = GetGame(IN_PROGRESS);
            if (player.NumberOfDice == 1 && game.Palifico)
            {
                game.NextRoundIsPalifico = true;
            } else {
                game.NextRoundIsPalifico = false;
            }
            _db.SaveChanges();
        }

        private async Task DecrementDieFromPlayerAndSetThierTurnAsync(Data.Game game, Player player, int penalty)
        {
            player.NumberOfDice -= penalty;

            if (player.NumberOfDice < 0) player.NumberOfDice = 0;

            if (player.NumberOfDice == 1 && game.Palifico)
            {
                game.NextRoundIsPalifico = true;
            }
            else
            {
                game.NextRoundIsPalifico = false;
            }

            if (player.NumberOfDice <= 0)
            {
                await SendMessage($":fire::skull::fire: {GetUserNickname(player.Username)} defeated :fire::skull::fire:");
                var deathrattle = _db.Rattles.SingleOrDefault(x => x.Username == player.Username);
                if (deathrattle != null)
                {
                    await SendMessage(deathrattle.Deathrattle);
                }

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

            if (game.FaceoffEnabled && players.Sum(x => x.NumberOfDice) == 2)
            {
                var allDice2 = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
                return allDice2.Sum();
            }

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));

            if (game.NextRoundIsPalifico)
            {
                return allDice.Count(x => x == pips);
            }
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

            if (mostRecentBid == null) {
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

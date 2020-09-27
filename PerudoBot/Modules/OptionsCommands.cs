using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
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
                $"`!option ghostexact/noghostexact` to allow ghosts to rejoin with 1 die if they make a successful exact call\n" +
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
            await Status();
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
                    var game = GetGame(GameState.Setup);

                    game.NumberOfDice = numberOfDice;
                    _db.SaveChanges();
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }
            else if (stringArray[0] == "penalty")
            {
                if (stringArray[1].ToLower() == "variable")
                {
                    var game = GetGame(GameState.Setup);

                    game.Penalty = 0;
                    _db.SaveChanges();
                }
                else
                {
                    var numberOfDice = int.Parse(stringArray[1]);

                    if (numberOfDice > 0 && numberOfDice <= 100)
                    {
                        var game = GetGame(GameState.Setup);

                        game.Penalty = numberOfDice;
                        _db.SaveChanges();
                    }
                }
                SetOptions(stringArray.Skip(2).ToArray());
            }
            else if (stringArray[0] == "randomized")
            {
                var game = GetGame(GameState.Setup);

                game.RandomizeBetweenRounds = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ghostexact")
            {
                var game = GetGame(SETUP);

                game.CanCallExactToJoinAgain = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noghostexact")
            {
                var game = GetGame(SETUP);

                game.CanCallExactToJoinAgain = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }

            else if (stringArray[0] == "ordered")
            {
                var game = GetGame(GameState.Setup);

                game.RandomizeBetweenRounds = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ranked")
            {
                var game = GetGame(GameState.Setup);

                game.IsRanked = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "unranked")
            {
                var game = GetGame(GameState.Setup);

                game.IsRanked = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "palifico")
            {
                var game = GetGame(GameState.Setup);

                game.Palifico = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nopalifico")
            {
                var game = GetGame(GameState.Setup);

                game.Palifico = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "faceoff")
            {
                var game = GetGame(GameState.Setup);

                game.FaceoffEnabled = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nofaceoff")
            {
                var game = GetGame(GameState.Setup);

                game.FaceoffEnabled = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nowild" || stringArray[0] == "nowilds")
            {
                var game = GetGame(GameState.Setup);

                game.WildsEnabled = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "wild" || stringArray[0] == "wilds")
            {
                var game = GetGame(GameState.Setup);

                game.WildsEnabled = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "bidanytime")
            {
                var game = GetGame(GameState.Setup);

                game.CanBidAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nobidanytime")
            {
                var game = GetGame(GameState.Setup);

                game.CanBidAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exactanytime")
            {
                var game = GetGame(GameState.Setup);

                game.CanCallExactAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noexactanytime")
            {
                var game = GetGame(GameState.Setup);
                game.ExactCallBonus = 0;
                game.ExactCallPenalty = 0;
                game.CanCallExactAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "liaranytime")
            {
                var game = GetGame(GameState.Setup);

                game.CanCallLiarAnytime = true;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noliaranytime")
            {
                var game = GetGame(GameState.Setup);

                game.CanCallLiarAnytime = false;
                _db.SaveChanges();

                SetOptions(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exact")
            {
                var game = GetGame(GameState.Setup);

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

        public List<string> GetOptions(Game game)
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
            if (game.CanCallExactToJoinAgain) options.Add("Defeated players have 1 chance to call exact to rejoin with 1 die (3+ players).");
            if (game.IsRanked) options.Add("Game is ranked and saved to highscore board.");

            return options;
        }
    }
}
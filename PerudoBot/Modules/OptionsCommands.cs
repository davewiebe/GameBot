﻿using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("option")]
        [Alias("options")]
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

            await SetOptionsAsync(stringArray);
            await Status();
        }

        private async Task SetOptionsAsync(string[] stringArray)
        {
            if (stringArray.Length == 0) return;

            var allText = string.Join(" ", stringArray).ToLower();
            allText = allText.Replace("simple", "nowild dice 5 penalty 1 noexactanytime noliaranytime nobidanytime exact 0 0 ordered nopalifico ranked");
            if (allText.Contains("suddendeath"))
            {
                allText = allText.Replace("suddendeath", "") + "nopalifico penalty 100 liaranytime";
            }
            allText = allText.Replace("chaos", "exactanytime liaranytime bidanytime");
            allText = allText.Replace("standard", "wild dice 5 penalty 1 noexactanytime noliaranytime nobidanytime exact 0 0 ordered palifico ranked ghostexact faceoff");

            stringArray = allText.Split(" ");
            if (stringArray[0] == "dice")
            {
                var numberOfDice = int.Parse(stringArray[1]);

                var game = await GetGameAsync(GameState.Setup);
                if (numberOfDice > 0 && numberOfDice <= 100)
                {

                    game.NumberOfDice = numberOfDice;
                }

                try
                {
                    var low = int.Parse(stringArray[2]);
                    var high = int.Parse(stringArray[3]);
                    if (low > 0 && high <= 9 && low < high)
                    {
                        game.LowestPip = low;
                        game.HighestPip = high;
                    }
                }
                catch { }
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(2).ToArray());
            }
            else if (stringArray[0] == "penalty")
            {
                if (stringArray[1].ToLower() == "variable")
                {
                    var game = await GetGameAsync(GameState.Setup);

                    game.Penalty = 0;
                    _db.SaveChanges();
                }
                else
                {
                    var numberOfDice = int.Parse(stringArray[1]);

                    if (numberOfDice > 0 && numberOfDice <= 100)
                    {
                        var game = await GetGameAsync(GameState.Setup);

                        game.Penalty = numberOfDice;
                        _db.SaveChanges();
                    }
                }
                await SetOptionsAsync(stringArray.Skip(2).ToArray());
            }
            else if (stringArray[0] == "randomized")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.RandomizeBetweenRounds = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ghostexact")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanCallExactToJoinAgain = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noghostexact")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanCallExactToJoinAgain = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ordered")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.RandomizeBetweenRounds = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "ranked")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.IsRanked = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "unranked")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.IsRanked = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "palifico")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.Palifico = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nopalifico")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.Palifico = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "faceoff")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.FaceoffEnabled = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nofaceoff")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.FaceoffEnabled = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nowild" || stringArray[0] == "nowilds")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.WildsEnabled = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "wild" || stringArray[0] == "wilds")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.WildsEnabled = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "bidanytime")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanBidAnytime = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "nobidanytime")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanBidAnytime = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exactanytime")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanCallExactAnytime = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noexactanytime")
            {
                var game = await GetGameAsync(GameState.Setup);
                game.ExactCallBonus = 0;
                game.ExactCallPenalty = 0;
                game.CanCallExactAnytime = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "liaranytime")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanCallLiarAnytime = true;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "noliaranytime")
            {
                var game = await GetGameAsync(GameState.Setup);

                game.CanCallLiarAnytime = false;
                _db.SaveChanges();

                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
            else if (stringArray[0] == "exact")
            {
                var game = await GetGameAsync(GameState.Setup);

                var exactCallerBonus = int.Parse(stringArray[1]);
                var exactOthersPenalty = int.Parse(stringArray[2]);

                if (exactCallerBonus >= 0 && exactCallerBonus <= 100 && exactOthersPenalty >= 0 && exactOthersPenalty <= 100)
                {
                    game.CanCallExactAnytime = true;
                    game.ExactCallBonus = exactCallerBonus;
                    game.ExactCallPenalty = exactOthersPenalty;
                    _db.SaveChanges();
                }

                await SetOptionsAsync(stringArray.Skip(3).ToArray());
            }
            else
            {
                await SetOptionsAsync(stringArray.Skip(1).ToArray());
            }
        }

    }
}
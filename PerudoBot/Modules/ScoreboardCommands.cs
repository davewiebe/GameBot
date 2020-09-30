using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("gamelog")]
        public async Task Gamelogs(params string[] stringArray)
        {
            var guildId = Context.Guild.Id;
            var players1 = _db.Players.AsQueryable()
                .Where(x => x.Game.IsRanked)
                .Where(x => x.Game.GuildId == guildId)
                .Where(x => x.Game.State == (int)(object)GameState.Finished)
                .Where(x => x.Game.Winner != null)
                .Include(x => x.Game.Notes)
                .OrderBy(x => x.Game.DateCreated)
                .ToList();

           var players = players1
                .GroupBy(x => x.Game).ToList();

            var skipNumber = 0;
            var page = 1;
            var i = -1;
            if (stringArray.Length == 1)
            {
                if (stringArray[0].ToLower().StartsWith("pg"))
                {
                    page = int.Parse(stringArray[0].Substring(2));
                    skipNumber = (page - 1) * 10;
                }
                else
                {
                    i = int.Parse(stringArray[0]);
                }
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
            var monkey = string.Join("\n", monk.Skip(skipNumber).Take(10));

            if (i == -1)
            {
                monkey += ("\n\nType `!gamelog 1` to get notes on a specific game");
                monkey += ("\nor `!gamelog pg2` to turn the page");
            }

            var pageText = "";
            if (page > 1) pageText = $"(pg{page})";

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs {pageText}")
                                .AddField("Games", string.Join("\n", monkey), inline: false);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            return;
        }

        [Command("gamelogs")]
        public async Task Gamelog(params string[] stringArray)
        {
            await Gamelogs(stringArray);
        }

        [Command("leaderboard")]
        public async Task Leaderboard(params string[] stringArray)
        {
            await SendMessage("`!leaderboard` has been depricated. Try `!gamelogs`");
        }
    }
}
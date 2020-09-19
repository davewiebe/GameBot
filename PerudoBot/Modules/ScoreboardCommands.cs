using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

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
            await Scoreboard(stringArray);
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
    }
}

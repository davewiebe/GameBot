using Discord;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class GamelogService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public GamelogService(GameBotDbContext db)
        {
            _db = db;
        }

        public string GetGamelog(ulong guildId, int page, int gamenumber)
        {
            var skipNumber = (page - 1) * 10;

            var players1 = _db.GamePlayers.AsQueryable()
                .Where(x => x.Game.IsRanked)
                .Where(x => x.Game.GuildId == guildId)
                .Where(x => x.Game.State == (int)(object)GameState.Finished)
                .Where(x => x.Game.Winner != null)
                .Include(x => x.Game.Notes)
                .OrderBy(x => x.Game.DateCreated)
                .ToList();

            var players = players1
                 .GroupBy(x => x.Game).ToList();

            var monk = new List<string>();
            var index = 1;
            foreach (var item in players)
            {
                if (gamenumber > -1)
                {
                    if (index != gamenumber)
                    {
                        index += 1;
                        continue;
                    }
                }
                var nonWinnerList = string.Join(", ", item.Where(x => x.Player.Username != item.Key.Winner).Select(x => x.Player.Username));
                monk.Add($"`{index.ToString("D2")}. {item.Key.DateCreated:yyyy-MM-dd}` :trophy: **{GetUserNickname(item.Key.Winner)}**, {nonWinnerList}");

                index += 1;

                if (gamenumber > -1)
                {
                    monk.AddRange(item.Key.Notes.Select(x => $"**{GetUserNickname(x.Username)}**: {x.Text}"));
                }
            }

            if (gamenumber == -1)
            {
                monk = monk.OrderByDescending(x => x).ToList();
            }
            var monkey = string.Join("\n", monk.Skip(skipNumber).Take(10));

            return string.Join("\n", monkey);
        }

        private string GetUserNickname(string username)
        {
            return username;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
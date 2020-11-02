using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Services
{
    public class StatsService
    {
        private readonly GameBotDbContext _db;

        public StatsService(GameBotDbContext db)
        {
            _db = db;
        }

        public int GetGamesWithRoundsCount(ulong guildId)
        {
            return _db.Games.AsQueryable()
                .Where(g => g.Rounds.Count() > 0)
                .Where(g => g.IsRanked)
                .Where(g => g.GuildId == guildId)
                .Count();
        }

        public List<TopActionsInAGameDto> GetTopActionsInAGame(ulong guildId, int topScoresToInclude = 1)
        {
            var topActionCountsInAGame = _db.Actions.AsQueryable()
                .AsNoTracking()
                .Include(x => x.Player)
                .Include(x => x.Round)
                .ThenInclude(x => x.Game)
                .Where(a => a.Round.Game.IsRanked)
                .Where(a => a.Round.Game.GuildId == guildId)
                .Where(a => (a.ActionType == nameof(Bid) && a.IsSuccess == true) || a.ActionType != nameof(Bid))
                .ToList()
                .GroupBy(x => new
                {
                    x.Round.GameId,
                    x.Player.Username,
                    x.IsSuccess,
                    x.IsOutOfTurn,
                    x.ActionType,
                    x.Round.Game.Penalty,
                    x.Round.Game.DateFinished,
                })
                .Select(g => new
                {
                    g.Key.GameId,
                    g.Key.Penalty,
                    g.Key.Username,
                    g.Key.IsSuccess,
                    g.Key.IsOutOfTurn,
                    g.Key.ActionType,
                    g.Key.DateFinished,
                    Count = g.Count()
                })
                .GroupBy(g => new
                {
                    g.ActionType,
                    g.IsSuccess,
                    g.IsOutOfTurn
                })
                .Select(g => new TopActionsInAGameDto
                {
                    ActionType = g.Key.ActionType,
                    IsSuccess = g.Key.IsSuccess,
                    IsOutOfTurn = g.Key.IsOutOfTurn,
                    TopRecords = g.Where(x => x.Count >=
                        g.OrderByDescending(x => x.Count)
                            .Select(x => x.Count)
                            .Distinct()
                            .Skip(topScoresToInclude - 1)
                            .Take(1)
                            .FirstOrDefault()
                        )
                        .Select(g => new TopRecord
                        {
                            Username = g.Username,
                            GameId = g.GameId,
                            GameDate = g.DateFinished,
                            Count = g.Count
                        })
                        .OrderByDescending(g => g.Count)
                        .ToList()
                })
                .OrderBy(g => g.ActionType)
                .ToList();

            return topActionCountsInAGame;
        }
    }

    public class TopActionsInAGameDto
    {
        public string ActionType { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsOutOfTurn { get; set; }
        public List<TopRecord> TopRecords { get; set; }
    }

    public class TopRecord
    {
        public string Username { get; set; }
        public int GameId { get; set; }
        public int Count { get; set; }
        public DateTime GameDate { get; set; }

        public override string ToString()
        {
            return $"{Count} - {Username,-14} {GameDate.ToShortDateString()} (Game {GameId})";
        }
    }
}
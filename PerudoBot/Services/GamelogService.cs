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

        public string GetGamelog(ulong guildId, int page, int gameId)
        {
            var pageSize = 20;
            var skipNumber = (page - 1) * pageSize;

            var gamesQuery = _db.Games.AsQueryable()
            .Where(x => x.IsRanked)
            .Where(x => x.GuildId == guildId)
            .Where(x => x.State == (int)GameState.Finished);

            if (gameId != -1)
                gamesQuery = gamesQuery.Where(g => g.Id == gameId);

            var games = gamesQuery
            .OrderByDescending(g => g.DateFinished)
            .Skip(skipNumber)
            .Take(pageSize)
            .Select(g => new
            {
                g.Id,
                g.DateFinished,
                Players = g.GamePlayers
                    .Select(gp => new
                    {
                        gp.Player.Username,
                        gp.Player.Nickname,
                        IsGhost = gp.GhostAttemptsLeft == -1,
                        IsWinner = gp.Player.Username == g.Winner,
                        gp.Rank
                    })
            })
            .Select(g => new
            {
                g.Id,
                g.DateFinished,
                Winner = g.Players.Where(p => p.IsWinner).Single(),
                NonWinners = g.Players.Where(p => !p.IsWinner).OrderBy(p => p.Rank).ToList(),
            })
            .ToList();

            var output = "";

            //only show a max number of non-winners, unless a specific gameId is given
            var maxNonWinnersToList = 4;
            if (gameId != -1)
            {
                maxNonWinnersToList = int.MaxValue;
            }

            foreach (var game in games)
            {
                var nonWinnerList = string.Join(", ",
                    game.NonWinners.Take(maxNonWinnersToList).Select(nw =>
                        $"{(nw.IsGhost ? ":ghost:" : "")} {nw.Nickname}")
                    );

                if (game.NonWinners.Count() > maxNonWinnersToList)
                {
                    nonWinnerList += $", *+{game.NonWinners.Count() - maxNonWinnersToList} more*";
                }

                output += $"`{game.Id:D3}. {game.DateFinished:yyyy-MM-dd}` " +
                    $":trophy: **{(game.Winner.IsGhost ? ":ghost:" : "")} " +
                    $"{game.Winner.Nickname}**, {nonWinnerList}\n";

                if (gameId != -1)
                {
                    var notes = _db.Notes.AsQueryable()
                        .Where(n => n.GameId == gameId)
                        .ToList()
                        // TODO: Link notes to GamePlayer/Player table to avoid this nonsense
                        .Select(n => $"**{GetUserNickname(n.Username)}**: {n.Text}")
                        ;

                    output += "\n" + string.Join("\n", notes);
                }
            }

            return output;
        }

        private string GetUserNickname(string username)
        {
            return _db.Players.AsQueryable()
                .Where(p => p.Username == username)
                .FirstOrDefault()
                .Nickname;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
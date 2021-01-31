using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public partial class PerudoGameSummaryService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public PerudoGameSummaryService(GameBotDbContext db)
        {
            _db = db;
        }

        public async Task<GameSummaryDto> GetGameSummaryAsync(int gameId)
        {
            return await _db.Games.AsQueryable()
                .Where(g => g.Id == gameId)
                .Select(g => new GameSummaryDto
                {
                    GameId = g.Id,
                    IsRanked = g.IsRanked,
                    DateStarted = g.DateStarted,
                    DateFinished = g.DateFinished,
                    DurationInSeconds = g.DurationInSeconds ?? 0,
                    RoundCount = g.Rounds.Count(),
                    Penalty = g.Penalty,
                    GameState = (GameState)g.State,
                    PlayerCount = g.GamePlayers.Count(),
                    Notes = g.Notes
                        .OrderBy(n => n.Id)
                        .Select(n => new NoteDto
                        {
                            Username = n.Username,
                            Text = n.Text
                        }).ToList(),
                    GamePlayers = g.GamePlayers
                        .OrderBy(gp => gp.Rank)
                        .Select(gp => new GamePlayerSummaryDto
                        {
                            Nickname = gp.Player.Nickname,
                            Rank = gp.Rank,
                            PostGameEloRating = gp.PostGameEloRating,
                            EloChange = gp.EloChange
                        }).ToList()
                }).SingleOrDefaultAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
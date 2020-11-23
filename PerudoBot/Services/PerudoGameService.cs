using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class PerudoGameService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public PerudoGameService(GameBotDbContext db)
        {
            _db = db;
        }

        public async Task TerminateGameAsync(int gameId)
        {
            var gameToTerminate = await _db.Games.AsQueryable()
                .SingleAsync(g => g.Id == gameId);

            if (gameToTerminate.State != (int)GameState.Finished)
            {
                gameToTerminate.State = (int)GameState.Terminated;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Game> GetGameAsync(ulong channelId, params GameState[] gameStates)
        {
            var gameStateIds = gameStates.Cast<int>().ToList();

            return await _db.Games.AsQueryable()
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.Actions)
                .Where(x => x.ChannelId == channelId)
                .Where(x => gameStateIds.Contains(x.State))
                .SingleOrDefaultAsync();
        }

        public async Task UpdateGamePlayerRanksAsync(int gameId)
        {
            var game = await _db.Games.AsQueryable()
                .Include(g => g.GamePlayers).ThenInclude(gp => gp.Player)
                .Include(g => g.Rounds).ThenInclude(r => r.GamePlayerRounds)
                .Where(g => g.Id == gameId)
                .SingleAsync();

            var rounds = game.Rounds.OrderBy(r => r.RoundNumber);

            var currentRank = game.GamePlayers.Count();

            foreach (var round in game.Rounds)
            {
                var playersEliminated = round.GamePlayerRounds
                    .Where(gpr => gpr.WasEliminated)
                    .Select(gpr => gpr.GamePlayer).ToList();

                foreach (var gamePlayer in playersEliminated)
                {
                    gamePlayer.Rank = currentRank;
                    currentRank--;
                }
            }

            var winningGamePlayer = game.GamePlayers
                .Where(gp => gp.Player.Username == game.Winner)
                .FirstOrDefault();

            if (winningGamePlayer != null)
            {
                winningGamePlayer.Rank = 1;
            }

            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
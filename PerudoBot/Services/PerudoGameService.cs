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

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
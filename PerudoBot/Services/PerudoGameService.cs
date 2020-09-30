using PerudoBot.Data;
using PerudoBot.Modules;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class PerudoGameService
    {
        public readonly GameBotDbContext _db;

        public PerudoGameService(GameBotDbContext db)
        {
            _db = db;
        }

        public async Task TerminateGame(int gameId)
        {
            var gameToTerminate = await _db.Games.SingleAsync(g => g.Id == gameId);

            gameToTerminate.State = (int)GameState.Terminated;

            await _db.SaveChangesAsync();
        }
    }
}

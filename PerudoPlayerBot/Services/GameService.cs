using Discord.Commands;
using Discord.WebSocket;
using PerudoPlayerBot.Data;
using System.Linq;

namespace PerudoPlayerBot.Services
{
    public class GameService
    {
        private SocketCommandContext _context;
        private PerudoPlayerBotDbContext _db;

        public GameService(SocketCommandContext context, PerudoPlayerBotDbContext db)
        {
            _context = context;
            _db = db;
        }

        public Round AddRoundIfNotExists()
        {
            var currentRound = GetCurrentRound();
            if (currentRound == null) currentRound = CreateNewRound();
            return currentRound;
        }

        public Round CreateNewRound()
        {
            Round lastRound;
            var game = _db.Games.First(x => x.Active == true);
            lastRound = new Round()
            {
                Active = true,
                GameId = game.Id
            };
            _db.Rounds.Add(lastRound);
            _db.SaveChanges();
            return lastRound;
        }

        public Round GetCurrentRound()
        {
            return _db.Rounds.AsQueryable()
                .Where(x => x.Game.Active == true)
                .Where(x => x.Active == true)
                .LastOrDefault();
        }

        public void DeactivatePreviousGames()
        {
            var activeGames = _db.Games.AsQueryable().Where(x => x.Active == true);
            foreach (var activeGame in activeGames)
            {
                activeGame.Active = false;
            }
            _db.SaveChanges();
        }
    }
}

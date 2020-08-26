using Discord.Commands;
using Discord.WebSocket;
using PerudoPlayerBot.Data;
using System.Collections.Generic;
using System.Linq;

namespace PerudoPlayerBot.Services
{
    public class PlayerService
    {
        private SocketCommandContext _context;
        private PerudoPlayerBotDbContext _db;

        public PlayerService(SocketCommandContext context, PerudoPlayerBotDbContext db)
        {
            _context = context;
            _db = db;
        }

        public Player AddPlayerIfNotExists(string botUsername)
        {
            var botPlayer = GetPlayer(botUsername);
            if (botPlayer == null)
            {
                botPlayer = CreateNewPlayer(botUsername);
            }

            return botPlayer;
        }

        private Player GetPlayer(string username)
        {
            return _db.Players.FirstOrDefault(x => x.Username == username);
        }

        private Player CreateNewPlayer(string botUsername)
        {
            Player botPlayer = new Player()
            {
                Username = botUsername
            };
            _db.Players.Add(botPlayer);
            _db.SaveChanges();
            return botPlayer;
        }
        public void CreatePlayersThatDontExist(List<string> usernameList)
        {
            var players = _db.Players.ToList();
            foreach (var username in usernameList)
            {
                var player = players.SingleOrDefault(x => x.Username == username);
                if (player == null)
                {
                    _db.Players.Add(new Player() { Username = username });
                }
            }
            _db.SaveChanges();
        }
    }
}

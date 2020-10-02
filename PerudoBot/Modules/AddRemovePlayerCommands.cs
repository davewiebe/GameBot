using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("add")]
        public async Task AddUserToGame(params string[] stringArray)
        {
            var game = await GetGameAsync(GameState.Setup);
            if (game == null)
            {
                await SendMessageAsync($"Unable to add players at this time.");
                return;
            }

            AddUsers(game, Context.Message);

            await Status();
        }

        private void AddUsers(Game game, SocketUserMessage message)
        {
            if (message.MentionedUsers.Count == 0)
            {
                AddUserToGame(game, message.Author.Username);
            }
            foreach (var userToAdd in message.MentionedUsers)
            {
                AddUserToGame(game, userToAdd.Username);
            }
        }

        private void AddUserToGame(Game game, string username)
        {
            bool userAlreadyExistsInGame = UserAlreadyExistsInGame(username, game);
            if (userAlreadyExistsInGame)
            {
                return;
            }

            _db.Players.Add(new Player
            {
                GameId = game.Id,
                Username = username,
                IsBot = GetUser(username).IsBot
            });

            _db.SaveChanges();
        }

        private bool UserAlreadyExistsInGame(string username, Game game)
        {
            var players = GetPlayers(game);
            bool userAlreadyExistsInGame = players.FirstOrDefault(x => x.Username == username) != null;
            return userAlreadyExistsInGame;
        }

        [Command("remove")]
        public async Task RemoveUserFromGame(string user)
        {
            var userToAdd = Context.Message.MentionedUsers.First();

            var game = await GetGameAsync(GameState.Setup);
            if (game == null)
            {
                await SendMessageAsync($"Unable to remove players at this time.");
                return;
            }

            var userToRemove = _db.Players.FirstOrDefault(x => x.GameId == game.Id && x.Username == userToAdd.Username);
            if (userToRemove == null) return;

            _db.Players.Remove(userToRemove);
            _db.SaveChanges();

            await SendMessageAsync($"{GetUserNickname(userToAdd.Username)} removed from game.");
        }
    }
}
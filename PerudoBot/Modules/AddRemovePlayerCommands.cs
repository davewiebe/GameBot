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
        public async Task AddUserToGameAsync(params string[] stringArray)
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
                AddUserToGame(game, (SocketGuildUser)message.Author);
            }
            foreach (var userToAdd in message.MentionedUsers)
            {
                AddUserToGame(game, userToAdd as SocketGuildUser);
            }
        }

        private void AddUserToGame(Game game, SocketGuildUser user)
        {
            // TODO: Can't add bots with current code
            bool userAlreadyExistsInGame = UserAlreadyExistsInGame(user.Username, game);
            if (userAlreadyExistsInGame)
            {
                return;
            }

            // get player
            // TODO: replace Username with UserId lookup when all user Ids are populated
            var player = _db.Players.AsQueryable()
                .Where(p => p.GuildId == Context.Guild.Id)
                .Where(p => p.Username == user.Username)
                .FirstOrDefault();

            if (player != null) // update player
            {
                player.Nickname = user.Nickname;
                player.UserId = user.Id;
            }
            else // create a new player
            {
                player = new Player
                {
                    Username = user.Username,
                    Nickname = user.Nickname,
                    GuildId = user.Guild.Id,
                    UserId = user.Id,
                    IsBot = user.IsBot
                };
            }

            _db.GamePlayers.Add(new GamePlayer
            {
                Game = game,
                Player = player
            });

            _db.SaveChanges();
        }

        private bool UserAlreadyExistsInGame(string username, Game game)
        {
            var players = GetPlayers(game);
            bool userAlreadyExistsInGame = players.FirstOrDefault(x => x.Player.Username == username) != null;
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

            var userToRemove = _db.GamePlayers.FirstOrDefault(x => x.GameId == game.Id && x.Player.Username == userToAdd.Username);
            if (userToRemove == null) return;

            _db.GamePlayers.Remove(userToRemove);
            _db.SaveChanges();

            await SendMessageAsync($"{GetUserNickname(userToAdd.Username)} removed from game.");
        }
    }
}
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
        [Alias("a", "join", "j")]
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
                _perudoGameService.AddUserToGame(game, (SocketGuildUser)message.Author);
            }
            foreach (var userToAdd in message.MentionedUsers)
            {
                var socketGuildUser = Context.Guild.GetUser(userToAdd.Id);
                _perudoGameService.AddUserToGame(game, socketGuildUser);
            }
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
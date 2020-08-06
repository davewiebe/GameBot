using Discord.Commands;
using GameBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace GameBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewGame()
        {
            if (_botType != "perudo") return;

            await ReplyAsync($"New game created. Add players with \"!add @user\".");
        }

        [Command("add")]
        public async Task AddUserToGame(string user)
        {
            if (_botType != "perudo") return;

            var userToAdd = Context.Message.MentionedUsers.First();

            await ReplyAsync($"{userToAdd.Username} added to game. Start the game with \"!start\"");
        }

        [Command("start")]
        public async Task Start()
        {
            if (_botType != "perudo") return;

            await ReplyAsync($"Starting the game! You should all have your dice messaged to you.");
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("karma")]
        public async Task Karma()
        {
            if (Context.Message.MentionedUsers.Count != 1) return;
            var splitskis = Context.Message.Content.Split('>');
            if (splitskis.Length != 2) return;
            var action = splitskis.Last().Trim();

            var user = Context.Message.MentionedUsers.First();
            if (action == "++" || action == "+=1")
            {
                await ReplyAsync($"Booya! Extra karma for {user.Username}");
                return;
            }
            else if (action == "--" || action == "-=1")
            {
                await ReplyAsync($"Shucks. Less karma for {user.Username}");
                return;
            }
        }
    }
}

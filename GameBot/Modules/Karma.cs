using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Services;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("karma")]
        public async Task Karma()
        {
            var karmaService = new KarmaService(Context);

            var text = Context.Message.Content;
            var karma = karmaService.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;
            var user = karmaService.GetUserFromText(text);

            if (user != null)
            {
                // Give karma to user
                await ReplyAsync($"{karma} karma for {user.Nickname ?? user.Username}");
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                // Give karma to object
                await ReplyAsync($"{karma} karma for {text}");
            }
        }

    }
}

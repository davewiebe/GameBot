using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("spreadkarma")]
        public async Task SpreadKarma()
        {
            var userService = new UserService(Context);
            var karmaService = new KarmaService(Context, _db);

            var text = Context.Message.Content;
            var karmaPoints = KarmaExtensions.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;
            var user = userService.GetUserFromText(text);

            if (user != null)
            {
                await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), karmaPoints));
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                await ReplyAsync(karmaService.GiveKarma(text, karmaPoints));
            }
        }


        [Command("karma")]
        public async Task Karma()
        {
            var userService = new UserService(Context);

            var scores = _db.Karma.ToList()
                .GroupBy(x => x.Thing)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"{userService.GetNicknameIfUser(x.Key)}: {x.Sum(y => y.Points)} karma");


            await ReplyAsync("Karma leaderboard:\n\n" + string.Join("\n", scores));
        }
    }
}

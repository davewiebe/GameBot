using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("givekarma")]
        public async Task GiveKarma()
        {
            var userService = new UserService(Context);
            var karmaService = new KarmaService(Context, _db);

            var text = Context.Message.Content;
            var karmaPoints = KarmaExtensions.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;
            var user = userService.GetUserFromText(text);

            if (user != null)
            {
                if (user.Id == Context.Message.Author.Id)
                {
                    await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), karmaPoints));
                    Thread.Sleep(4000);
                    await ReplyAsync("Hold up... I see what you did there");
                    var minus = $"{userService.GetNicknameIfUser(user.Id.ToString())}-- ";
                    await ReplyAsync($"{minus}{minus}{minus}{minus}{minus}");
                    karmaService.GiveKarma(user.Id.ToString(), -4);
                    await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), -1));
                    return;
                }

                if (user.Id == Context.Client.CurrentUser.Id)
                {
                    if (!karmaService.HasGivenKarmaRecently(user.Id.ToString(), 10080))
                    {
                        await ReplyAsync("Awe, thanks. Right back at you");
                        await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), karmaPoints));
                    }
                    else
                    {
                        await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), karmaPoints));
                        await ReplyAsync("Yeah buddy.");
                    }
                }

                if (karmaService.HasGivenKarmaRecently(user.Id.ToString(), 5))
                {
                    await ReplyAsync("Slow down, buddy.");
                    return;
                }

                await ReplyAsync(karmaService.GiveKarma(user.Id.ToString(), karmaPoints));
                return;
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                if (karmaService.HasGivenKarmaRecently(text, 5))
                {
                    await ReplyAsync("Slow down, buddy.");
                    return;
                }

                await ReplyAsync(karmaService.GiveKarma(text, karmaPoints));
                return;
            }
        }


        [Command("karma")]
        public async Task Karma()
        {
            var userService = new UserService(Context);

            var scores = _db.Karma.ToList()
                .GroupBy(x => x.Thing, StringComparer.InvariantCultureIgnoreCase)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"{userService.GetNicknameIfUser(x.Key)}: {x.Sum(y => y.Points)} karma");


            await ReplyAsync("Karma leaderboard:\n\n" + string.Join("\n", scores));
        }
    }
}

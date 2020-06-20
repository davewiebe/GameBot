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
            var text = Context.Message.Content;
            var karmaPoints = KarmaExtensions.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;
            var user = _userService.GetUserFromText(text);

            var fromUser = Context.Message.Author;
            var appDevBot = Context.Client.CurrentUser;

            int totalPoints;


            if (user != null)
            {
                if (_karmaService.HasGivenKarmaRecently(user.Id.ToString(), 5))
                {
                    await ReplyAsync("Slow down, buddy.");
                    return;
                }

                if (user.Id == fromUser.Id)
                {
                    await ManipulatingOwnKarma(user, karmaPoints);
                    return;
                }

                if (user.Id == appDevBot.Id)
                {
                    if (!_karmaService.HasGivenKarmaRecently(user.Id.ToString(), 10080))
                    {
                        _karmaService.SaveKarma(user.Id.ToString(), karmaPoints, fromUser.Id);
                        totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
                        await ReplyAsync($"{user.Username}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");
                        await ReplyAsync("Awe, thanks. Right back at you"); // TODO, unless it went down!

                        _karmaService.SaveKarma(user.Id.ToString(), karmaPoints, fromUser.Id);
                        totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
                        await ReplyAsync($"{user.Nickname ?? user.Username}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");
                        return;
                    }
                    else
                    {
                        _karmaService.SaveKarma(user.Id.ToString(), karmaPoints, fromUser.Id);
                        totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
                        await ReplyAsync($"{user.Username}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");
                        await ReplyAsync("Yeah buddy."); // TODO, UNLESS IT WENT DOWN
                        return;
                    }
                }

                _karmaService.SaveKarma(user.Id.ToString(), karmaPoints, fromUser.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
                await ReplyAsync( $"{user.Nickname}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");
                return;
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                if (_karmaService.HasGivenKarmaRecently(text, 5))
                {
                    await ReplyAsync("Slow down, buddy.");
                    return;
                }

                _karmaService.SaveKarma(text, karmaPoints, fromUser.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(text);
                await ReplyAsync($"{text}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");
                return;
            }
        }

        private async Task ManipulatingOwnKarma(SocketGuildUser user, int karmaPoints)
        {
            int totalPoints;
            _karmaService.SaveKarma(user.Id.ToString(), karmaPoints, user.Id);
            totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
            await ReplyAsync($"{user.Nickname}'s karma has {(karmaPoints > 0 ? "increased" : "decreased")} to {totalPoints}");

            if (karmaPoints > 0)
            {
                await ReplyAsync("Hold up... I see what you did there");

                var minus = $"{user.Nickname}-- ";
                await ReplyAsync($"{minus}{minus}{minus}{minus}{minus}");

                _karmaService.SaveKarma(user.Id.ToString(), -5, Context.Client.CurrentUser.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(user.Id.ToString());
                await ReplyAsync($"{user.Nickname}'s karma has decreased to {totalPoints}");
            }
        }

        [Command("karma")]
        public async Task Karma()
        {
            var scores = _db.Karma.ToList()
                .GroupBy(x => x.Thing, StringComparer.InvariantCultureIgnoreCase)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"{_userService.GetNicknameIfUser(x.Key)}: {x.Sum(y => y.Points)} karma");


            await ReplyAsync("Karma leaderboard:\n\n" + string.Join("\n", scores));
        }

        [Command("karma")]
        public async Task Karma(string thing)
        {
            var scores = _db.Karma.ToList()
                .GroupBy(x => x.Thing, StringComparer.InvariantCultureIgnoreCase)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"{_userService.GetNicknameIfUser(x.Key)}: {x.Sum(y => y.Points)} karma");


            await ReplyAsync("Karma leaderboard:\n\n" + string.Join("\n", scores));
        }
    }
}

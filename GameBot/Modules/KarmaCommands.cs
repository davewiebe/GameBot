using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Enums;
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
            _karmaService = new KarmaService(Context, _db);
            _userService = new UserService(Context);

            var text = Context.Message.Content;
            var fromUser = Context.Guild.GetUser(Context.Message.Author.Id);
            var karmaPoints = KarmaExtensions.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;

            if (HasGivenKarmaRecently(text, 5))
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.SlowDown));
                return;
            }

            var user = _userService.TryGetUserFromText(text);
            if (user != null)
            {
                if (user.Id == fromUser.Id)
                {
                    await ManipulatingOwnKarma(fromUser, karmaPoints);
                    return;
                }

                if (user.Id == Context.Client.CurrentUser.Id)
                {
                    await GivingAppDevBotKarma(fromUser, karmaPoints);
                    return;
                }

                _karmaService.SaveKarma(user.Id, karmaPoints, fromUser.Id);
                var totalPoints = _karmaService.GetTotalKarmaPoints(user.Id);
                if (karmaPoints > 0)
                {
                    await ReplyAsync($"{user.Nickname}'s karma has increased to {totalPoints}");
                }
                else
                {
                    await ReplyAsync($"{user.Nickname}'s karma has decreased to {totalPoints}");
                }
                return;
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                _karmaService.SaveKarma(text, karmaPoints, fromUser.Id);
                var totalPoints = _karmaService.GetTotalKarmaPoints(text);
                if (karmaPoints > 0)
                {
                    await ReplyAsync($"{text}'s karma has increased to {totalPoints}");
                } else
                {
                    await ReplyAsync($"{text}'s karma has decreased to {totalPoints}");
                }
                return;
            }
        }

        private async Task GivingAppDevBotKarma(SocketGuildUser fromUser, int karmaPoints)
        {
            var appDevBot = Context.Client.CurrentUser;
            _karmaService.SaveKarma(appDevBot.Id, karmaPoints, fromUser.Id);
            var totalPoints = _karmaService.GetTotalKarmaPoints(appDevBot.Id);

            if (karmaPoints > 0)
            {
                await ReplyAsync($"{appDevBot.Username}'s karma has increased to {totalPoints}");

                if (_karmaService.HasGivenKarmaRecently(appDevBot.Id, 10080))
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.ThankYouFromBot));
                } else { 
                    await ReplyAsync("Right back at you.");
                    _karmaService.SaveKarma(fromUser.Id, karmaPoints, appDevBot.Id);
                    totalPoints = _karmaService.GetTotalKarmaPoints(fromUser.Id);
                    await ReplyAsync($"{fromUser.Nickname}'s karma has increased to {totalPoints}");
                }
            }
            else
            {
                await ReplyAsync($"{appDevBot.Username}'s karma has decreased to {totalPoints}");
                await ReplyAsync("I thought we were friends");
            }
            return;
        }

        private bool HasGivenKarmaRecently(string text, int minutes)
        {
            var user = _userService.TryGetUserFromText(text);
            if (user == null) return _karmaService.HasGivenKarmaRecently(text, minutes);
            return _karmaService.HasGivenKarmaRecently(user.Id, minutes);
        }

        private async Task ManipulatingOwnKarma(SocketGuildUser user, int karmaPoints)
        {
            int totalPoints;
            _karmaService.SaveKarma(user.Id, karmaPoints, user.Id);
            totalPoints = _karmaService.GetTotalKarmaPoints(user.Id);
            if (karmaPoints > 0)
            {
                await ReplyAsync($"{user.Nickname}'s karma has increased to {totalPoints}");
                await ReplyAsync("Hold up... I see what you did there");

                var minus = $"{user.Nickname}-- ";
                await ReplyAsync($"{minus}{minus}{minus}{minus}{minus}");

                _karmaService.SaveKarma(user.Id, -5, Context.Client.CurrentUser.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(user.Id);
                await ReplyAsync($"{user.Nickname}'s karma has decreased to {totalPoints}");
            }
            else
            {
                await ReplyAsync($"{user.Nickname}'s karma has decreased to {totalPoints}");
            }
        }

        [Command("karma")]
        public async Task Karma()
        {
            _userService = new UserService(Context);

            var scores = _db.Karma.ToList()
                .GroupBy(x => x.Thing, StringComparer.InvariantCultureIgnoreCase)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"{_userService.GetNicknameIfUser(x.Key)}: {x.Sum(y => y.Points)} karma");


            await ReplyAsync("Karma leaderboard:\n\n" + string.Join("\n", scores));
        }

        [Command("karma")]
        public async Task Karma(string thing)
        {
            _userService = new UserService(Context);

            var karma = _db.Karma
                .AsQueryable()
                .Where(x => x.Thing.ToUpper() == thing.ToUpper())
                .Sum(x => x.Points);

            await ReplyAsync($"{_userService.GetNicknameIfUser(thing)} has {karma} karma");
        }
    }
}

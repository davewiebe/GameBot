using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Enums;
using GameBot.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
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

            _phraseService.AddReplacement("<from>", fromUser.Nickname);
            _phraseService.AddReplacement("<karma>", karmaPoints.ToString());
            _phraseService.AddReplacement("<thing>", _userService.GetNicknameIfUser(text));
            _phraseService.AddReplacement("<bot>", Context.Client.CurrentUser.Username);

            if (HasGivenKarmaRecently(text, maxKarma:3, minutes:5))
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
                _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());
                if (karmaPoints > 0)
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaIncreased));
                }
                else
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
                }
                return;
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                _karmaService.SaveKarma(text, karmaPoints, fromUser.Id);
                var totalPoints = _karmaService.GetTotalKarmaPoints(text);
                _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());
                if (karmaPoints > 0)
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaIncreased));
                } else
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
                }
                return;
            }
        }

        private async Task GivingAppDevBotKarma(SocketGuildUser fromUser, int karmaPoints)
        {
            var appDevBot = Context.Client.CurrentUser;
            _karmaService.SaveKarma(appDevBot.Id, karmaPoints, fromUser.Id);
            var totalPoints = _karmaService.GetTotalKarmaPoints(appDevBot.Id);
            _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());

            if (karmaPoints > 0)
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaIncreased));

                if (_karmaService.HasGivenTooMuchKarmaRecently(appDevBot.Id, 1, 10080))
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.ThankYouFromBot));
                } else
                {
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.RightBackAtYou));
                    _karmaService.SaveKarma(fromUser.Id, karmaPoints, appDevBot.Id);
                    totalPoints = _karmaService.GetTotalKarmaPoints(fromUser.Id);

                    _phraseService.AddReplacement("<from>", appDevBot.Username);
                    _phraseService.AddReplacement("<thing>", fromUser.Nickname);
                    _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());
                    await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaIncreased));
                }
            }
            else
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.BotIsHurt));

                _karmaService.SaveKarma(fromUser.Id, karmaPoints, appDevBot.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(fromUser.Id);

                _phraseService.AddReplacement("<from>", appDevBot.Username);
                _phraseService.AddReplacement("<thing>", fromUser.Nickname);
                _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
            }
            return;
        }

        private bool HasGivenKarmaRecently(string text, int maxKarma, int minutes)
        {
            var user = _userService.TryGetUserFromText(text);
            if (user != null) text = user.Id.ToString();
            return _karmaService.HasGivenTooMuchKarmaRecently(text, maxKarma, minutes);
        }

        private async Task ManipulatingOwnKarma(SocketGuildUser user, int karmaPoints)
        {
            int totalPoints;
            _karmaService.SaveKarma(user.Id, karmaPoints, user.Id);
            totalPoints = _karmaService.GetTotalKarmaPoints(user.Id);
            _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());
            if (karmaPoints > 0)
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaIncreased));
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.HoldUp));

                var minus = $"{user.Nickname}-- ";
                await ReplyAsync($"{minus}{minus}{minus}{minus}{minus}");

                _karmaService.SaveKarma(user.Id, -5, Context.Client.CurrentUser.Id);
                totalPoints = _karmaService.GetTotalKarmaPoints(user.Id);
                _phraseService.AddReplacement("<totalkarma>", totalPoints.ToString());

                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
            }
            else
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.KarmaDecreased));
            }
        }

        [Command("karma")]
        public async Task Karma()
        {
            _userService = new UserService(Context);

            var karmas = _db.Karma
                .AsQueryable()
                .Where(x => x.Server == Context.Guild.Id)
                .ToList();

            var userScores = karmas
                .Where(x => x.Thing != _userService.GetNicknameIfUser(x.Thing))
                .GroupBy(x => _userService.GetNicknameIfUser(x.Thing), StringComparer.InvariantCultureIgnoreCase)
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"`{x.Sum(y => y.Points)} karma` - {x.Key}");


            var objectScores = karmas
                .Where(x => x.Thing == _userService.GetNicknameIfUser(x.Thing))
                .GroupBy(x => _userService.GetNicknameIfUser(x.Thing), StringComparer.InvariantCultureIgnoreCase);

            var topObjectScores = objectScores
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Take(5)
                .ToList()
                .Select(x => $"`{x.Sum(y => y.Points)} karma` - {NewMethod(x.Key)}");

            var take = 4;
            if (objectScores.ToList().Count < 10)
            {
                take = objectScores.ToList().Count - 5;
            }

            var bottomObjectScores = objectScores
                .OrderBy(x => x.Sum(y => y.Points))
                .Take(4)
                .ToList()
                .OrderByDescending(x => x.Sum(y => y.Points))
                .Select(x => $"`{x.Sum(y => y.Points)} karma` - {NewMethod(x.Key)}");



            var builder = new EmbedBuilder()
                .WithTitle("Karma leaderboard")
                .AddField("Users", string.Join("\n", userScores), inline: true)
                .AddField("Non-users", string.Join("\n", topObjectScores) + "\n...\n" + string.Join("\n", bottomObjectScores), inline: true);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

        private static string NewMethod(string x)
        {
            return (x.Length > 16) ? x.Substring(0, 15) + "..." : x;
        }

        [Command("karma")]
        public async Task Karma(string thing)
        {
            _userService = new UserService(Context);

            var user = _userService.TryGetUserFromText(thing);
            if (user != null) thing = user.Id.ToString();

            var karmaQuery = _db.Karma
                .AsQueryable()
                .Where(x => x.Server == Context.Guild.Id)
                .Where(x => x.Thing.ToUpper() == thing.ToUpper());

            var karma = karmaQuery.Sum(x => x.Points);

            var fromUser = Context.Guild.GetUser(Context.Message.Author.Id);
            _phraseService.AddReplacement("<from>", fromUser.Nickname);
            _phraseService.AddReplacement("<bot>", Context.Client.CurrentUser.Username);
            _phraseService.AddReplacement("<totalkarma>", karma.ToString());
            _phraseService.AddReplacement("<thing>", _userService.GetNicknameIfUser(thing));

            await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.GetCurrentKarma));
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            var karmaPoints = karmaService.RemoveKarmaFromText(ref text);
            if (text.Contains(' ')) return;
            var user = karmaService.GetUserFromText(text);

            if (user != null)
            {
                var karma = new Karma
                {
                    Points = karmaPoints,
                    Thing = user.Id.ToString(),
                    FromUserId = Context.Message.Author.Id,
                    GivenOn = DateTime.Now
                };
                _db.Karma.Add(karma);
                _db.SaveChanges();
                await ReplyAsync($"{karmaPoints} karma for {user.Nickname ?? user.Username}");
            }
            else if (Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                var karma = new Karma
                {
                    Points = karmaPoints,
                    Thing = text,
                    FromUserId = Context.Message.Author.Id,
                    GivenOn = DateTime.Now
                };
                _db.Karma.Add(karma);
                _db.SaveChanges();
                await ReplyAsync($"{karmaPoints} karma for {text}");
            }
        }


        [Command("karmaboard")]
        public async Task Karmaboard()
        {
            //_db.Karma.ToList()
            //    .GroupBy(x => x.Thing)
            //    .Select(x => x.Ke;
            await ReplyAsync("Pong");
        }

    }
}

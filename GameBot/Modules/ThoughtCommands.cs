using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Enums;
using GameBot.Services;
using RedditSharp;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("thoughtsxyz")]
        public async Task Thoughts(params String[] stringArray)
        {
            if (_botType != "deepthought") return;

            if (Context.Guild == null) await ReplyAsync("Sorry, I only reply to public audiences.");

            var channel = Context.Guild.GetChannel(Context.Channel.Id);
            if (channel.PermissionOverwrites.Count > 0)
            {
                await ReplyAsync("Sorry. This channel is too exclusive for me.");
                return;
            }

            var skipX = 30;
            var r = new Random();

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit("/r/showerthoughts");

            while (skipX < 50)
            {
                var skipNumber = r.Next(0, skipX);

                var showerthought2 = subreddit.Posts
                    .Where(x => x.IsStickied == false)
                    .Where(x => x.NSFW == false)
                    .Skip(skipNumber)
                    .Take(1)
                    .First();

                if (_db.DeepThoughts.FirstOrDefault(x => x.PostId == showerthought2.Id) == null)
                {
                    _db.DeepThoughts.Add(new DeepThought { PostId = showerthought2.Id });
                    _db.SaveChanges();
                    await ReplyAsync(showerthought2.Title);
                    return;
                }
                skipX += 1;
            }
        }

        [Command("deepxyz")]
        public async Task Deep(params String[] stringArray)
        {
            if (_botType != "marvin") return;

            if (Context.Guild == null) await ReplyAsync("Sorry, I only reply to public audiences.");

            var channel = Context.Guild.GetChannel(Context.Channel.Id);
            if (channel.PermissionOverwrites.Count > 0)
            {
                await ReplyAsync("Sorry. This channel is too exclusive for me.");
                return;
            }

            var skipX = 30;
            var r = new Random();

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit("/r/deepthoughts");

            while (skipX < 100)
            {
                var skipNumber = r.Next(0, skipX);

                var showerthought2 = subreddit.Posts
                    .Where(x => x.IsStickied == false)
                    .Where(x => x.NSFW == false)
                    .Where(x => x.SelfText == "")
                    .Skip(skipNumber)
                    .Take(1)
                    .First();

                if (_db.DeepThoughts.FirstOrDefault(x => x.PostId == showerthought2.Id) == null)
                {
                    _db.DeepThoughts.Add(new DeepThought { PostId = showerthought2.Id });
                    _db.SaveChanges();
                    await ReplyAsync(showerthought2.Title);
                    return;
                }
                skipX += 5;
            }
        }
    }
}

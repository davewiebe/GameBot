using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Data;
using RedditSharp;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("thoughtsxyz")]
        public async Task Thoughts(params String[] stringArray)
        {
            if (_botType != "deepthought") return;

            await ReplyAsync(GetSubredditPost("showerthoughts"));
        }
        public static string StripPrefix(string text, string prefix)
        {
            return text.ToLower().StartsWith(prefix.ToLower()) ? text.Substring(prefix.Length) : text;
        }

        [Command("tilxyz")]
        public async Task Til(params String[] stringArray)
        {
            if (_botType != "til") return;

            var monkey = GetSubredditPost("todayilearned").Trim();
            //strip
            monkey = StripPrefix(monkey, "TIL that").Trim();
            monkey = StripPrefix(monkey, "TIL of").Trim();
            monkey = StripPrefix(monkey, "TIL:").Trim();
            monkey = StripPrefix(monkey, "TIL-").Trim();
            monkey = StripPrefix(monkey, "TIL - ").Trim();
            monkey = StripPrefix(monkey, "TIL :").Trim();
            monkey = StripPrefix(monkey, "TIL").Trim();

            // capialize first char


            await ReplyAsync(FirstLetterToUpper(monkey.Trim()));
        }
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        private string GetSubredditPost(string subredditName)
        {
            if (Context.Guild == null) return "Sorry, I only reply to public audiences.";

            var channel = Context.Guild.GetChannel(Context.Channel.Id);
            if (channel.PermissionOverwrites.Count > 0)
            {
                return "Sorry. This channel is too exclusive for me.";
            }

            var skipX = 30;
            var r = new Random();

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit($"/r/{subredditName}");

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
                    return showerthought2.Title;
                }
                skipX += 1;
            }
            return "";
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

                var deepthought = subreddit.GetTop(RedditSharp.Things.FromTime.All)
                    .Where(x => x.IsStickied == false)
                    .Where(x => x.NSFW == false)
                    .Where(x => x.SelfText == "")
                    .Skip(skipNumber)
                    .Take(1)
                    .First();

                if (_db.DeepThoughts.FirstOrDefault(x => x.PostId == deepthought.Id) == null)
                {
                    _db.DeepThoughts.Add(new DeepThought { PostId = deepthought.Id });
                    _db.SaveChanges();
                    await ReplyAsync(deepthought.Title);
                    return;
                }
                skipX += 5;
            }
        }
    }
}

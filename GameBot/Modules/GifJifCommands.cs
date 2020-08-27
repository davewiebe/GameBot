using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenorSharp;
using TenorSharp.ResponseObjects;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("jifxyz")]
        public async Task Jif(params String[] stringArray)
        {
            if (_botType != "jif") return;

            var r = new Random();
            var words = new List<string>() { "pb&j", "jif", "peanut jam", "peanut jelly", "jif" };
            var word = words.ElementAt(r.Next(0, words.Count()));

            await TenorSearch($"{word} {string.Join(" ", stringArray)}");
        }


        [Command("gifxyz")]
        public async Task Gif(params String[] stringArray)
        {
            if (_botType != "gif") return;

            await TenorSearch(stringArray);

        }

        private async Task TenorSearch(params string[] stringArray)
        {
            var tenor = new TenorClient(_tenorToken);
            tenor.SetContentFilter(TenorSharp.Enums.ContentFilter.high);
            GifObject[] gifs = new GifObject[0];
            var range = 50;
            while (range > 1)
            {
                try
                {
                    gifs = tenor.Search(string.Join(" ", stringArray), range).GifResults;
                    range = 0;
                }
                catch
                {
                    range /= 2;
                }
            }

            if (gifs.Count() == 0)
            {
                gifs = tenor.Search("confused").GifResults;
            }

            var r = new Random();
            var gif = gifs.ElementAt(r.Next(0, gifs.Count()));

            await ReplyAsync(gif.Url.OriginalString);
        }
    }
}

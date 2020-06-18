using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBot.Services
{
    public static class KarmaExtensions
    {
        public static int RemoveKarmaFromText(ref string text)
        {
            var karma = 0;
            if (text.EndsWith("++"))
            {
                karma += 1;
                text = text.Substring(0, text.Length - 2).Trim();
            }
            else if (text.EndsWith("+=1"))
            {
                karma += 1;
                text = text.Substring(0, text.Length - 3).Trim();
            }
            else if (text.EndsWith("-=1"))
            {
                karma += -1;
                text = text.Substring(0, text.Length - 3).Trim();
            }
            else if (text.EndsWith("--"))
            {
                karma += -1;
                text = text.Substring(0, text.Length - 2).Trim();
            }

            return karma;
        }
    }
}

using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBot.Services
{
    public class KarmaService
    {
        private SocketCommandContext _context;
        public KarmaService(SocketCommandContext context)
        {
            _context = context;
        }

        public SocketGuildUser GetUserFromText(string text)
        {
            SocketGuildUser user = null;
            if (text.StartsWith("<@") && text.EndsWith(">"))
            {
                var userIdString = text.Trim(new char[] { '<', '>', '@', '!' });
                var userId = ulong.Parse(userIdString);
                user = _context.Guild.GetUser(userId);
            }

            return user;
        }

        public int RemoveKarmaFromText(ref string text)
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

using Discord.Commands;
using Discord.WebSocket;

namespace GameBot.Services
{
    public class UserService
    {
        private SocketCommandContext _context;
        public UserService(SocketCommandContext context)
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
        public string GetNicknameIfUser(string value)
        {
            if (ulong.TryParse(value, out ulong result))
            {
                var user = _context.Guild.GetUser(result);
                if (user != null)
                {
                    return user.Nickname ?? user.Username;
                }
            }
            return value;
        }
    }
}

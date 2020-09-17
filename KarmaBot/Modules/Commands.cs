using Discord.Commands;
using GameBot.Data;
using GameBot.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private GameBotDbContext _db;
        private KarmaService _karmaService;
        private PhraseService _phraseService;
        private UserService _userService;

        public Commands()
        {
            _db = new GameBotDbContext();
            _phraseService = new PhraseService(_db);
        }
    }
}

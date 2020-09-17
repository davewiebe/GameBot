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
        private UserService _userService;
        private PhraseService _phraseService;
        private PerudoMessageParserService _perudoMessageParserService;
        private readonly AudioService _audioService;
        private string _tenorToken;
        private string _botType;

        public Commands(AudioService service)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

            _tenorToken = configuration.GetSection("TenorToken").Value;
            _botType = configuration.GetSection("BotType").Value;

            _db = new GameBotDbContext();
            _phraseService = new PhraseService(_db);
            _perudoMessageParserService = new PerudoMessageParserService(_db);

            _audioService = service;
        }
    }
}

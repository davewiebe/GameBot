using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PerudoPlayerBot.Data;
using System.IO;
using System.Linq;

namespace PerudoPlayerBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private PerudoPlayerBotDbContext _db;
        private string _aesEncryptionKey;
        private string _perudoBotUsername;

        public Commands()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

            _aesEncryptionKey = configuration.GetSection("AesEncryptionKey").Value;
            _perudoBotUsername = configuration.GetSection("PerudoBotUsername").Value;

            _db = new PerudoPlayerBotDbContext();
        }
    }
}

using Discord.Commands;
using GameBot.Data;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private GameBotDbContext _db;
        private string _botType;

        public Commands()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

            _botType = configuration.GetSection("BotType").Value;

            _db = new GameBotDbContext();
        }
    }
}

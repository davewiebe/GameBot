using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private string _tenorToken;
        private string _botType;

        public Commands()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

            _tenorToken = configuration.GetSection("TenorToken").Value;
            _botType = configuration.GetSection("BotType").Value;
        }
    }
}

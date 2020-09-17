using Discord.Commands;
using PerudoBot.Services;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _audioService;

        public Commands(AudioService service)
        {
            _audioService = service;
        }
    }
}

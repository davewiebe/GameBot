using Discord.Commands;
using GameBot.Services;

namespace GameBot.Modules
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

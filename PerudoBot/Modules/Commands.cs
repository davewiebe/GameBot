using Discord.Commands;
using PerudoBot.Data;
namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private GameBotDbContext _db;

        public Commands()
        {
            _db = new GameBotDbContext();
        }
    }
}

using Discord.Commands;
using PerudoBot.Data;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("terminate")]
        public async Task Terminate(params string[] bidText)
        {
            var game = GetGame(IN_PROGRESS);
            if (game != null) game.State = TERMINATED;

            game = GetGame(SETUP);
            if (game != null) game.State = TERMINATED;

            _db.SaveChanges();

            await SendMessage("I'll be back.");
        }
    }
}

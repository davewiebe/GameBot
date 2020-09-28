using Discord.Commands;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("terminate")]
        public async Task Terminate()
        {
            var game = GetGame(GameState.InProgress, GameState.Setup);

            if (game != null)
            {
                game.State = (int)GameState.Terminated;
                _db.SaveChanges();
                await SendMessage("I'll be back.");
                return;
            }

            await SendMessage("No games to terminate.");


        }
    }
}
using Discord.Commands;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("terminate")]
        public async Task Terminate()
        {            
            var game = GetGame(GameState.Setup, GameState.InProgress);

            if (game != null)
            {
                await _perudoGameService.TerminateGame(game.Id);
                await SendMessage("I'll be back.");
                return;
            }

            await SendMessage("No games to terminate.");

        }
    }
}
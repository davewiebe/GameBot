﻿using Discord.Commands;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("terminate")]
        public async Task Terminate()
        {
            var game = await GetGameAsync(GameState.Setup, GameState.InProgress);

            if (game != null)
            {
                await _perudoGameService.TerminateGameAsync(game.Id);
                await SendMessageAsync("I'll be back.");
                return;
            }

            await SendMessageAsync("No games to terminate.");
        }
    }
}
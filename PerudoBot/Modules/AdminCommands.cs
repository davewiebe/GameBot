using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
using System.Linq;
using System.Threading.Tasks;
using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("update-ranks")]
        public async Task UpdateRanksAsync(params string[] parameters)
        {
            var gamesWithGamePlayerRounds =
                _db.Rounds.AsQueryable()
                    .Where(r => r.GamePlayerRounds.Any())
                    .Where(r => r.Game.State == 3)
                    .Select(r => r.Game)
                    .Distinct()
                    .ToList();

            var perudoGameService = new PerudoGameService(_db);

            await Context.Channel.SendMessageAsync("Updating ranks for games with GamePlayerRounds...");

            foreach (var game in gamesWithGamePlayerRounds)
            {
                await perudoGameService.UpdateGamePlayerRanksAsync(game.Id);
            }

            await Context.Channel.SendMessageAsync("Done.");
        }
    }
}
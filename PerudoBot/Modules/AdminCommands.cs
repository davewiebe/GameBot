using Discord.Commands;
using PerudoBot.Services;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("update-ranks")]
        public async Task UpdateRanksAsync(int gameId)
        {
            var gamesWithGamePlayerRounds =
                _db.Rounds.AsQueryable()
                    .Where(r => r.GamePlayerRounds.Any())
                    .Where(r => r.Game.State == 3)
                    .Where(r => r.GameId == gameId || gameId == 0)
                    .Select(r => r.Game)
                    .Distinct()
                    .ToList();

            var perudoGameService = new PerudoGameService(_db);

            await Context.Channel.SendMessageAsync("Updating ranks for games with GamePlayerRounds...");

            foreach (var game in gamesWithGamePlayerRounds)
            {
                Console.WriteLine($"Updating ranks for game {game.Id}");
                await perudoGameService.UpdateGamePlayerRanksAsync(game.Id);
            }

            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("generate-elo-all")]
        public async Task GenerateEloRatingsForAllGamesAsync()
        {
            var eloRatingService = new EloRatingService(_db);

            await Context.Channel.SendMessageAsync("Generating Elo Ratings for All Games...");

            await eloRatingService.GenerateEloRatingsForAllGamesAsync(Context.Guild.Id, true);

            await Context.Channel.SendMessageAsync("Done.");
        }

        [Command("generate-elo")]
        public async Task GenerateEloRatingsForGameAsync(int gameId)
        {
            var eloRatingService = new EloRatingService(_db);

            await Context.Channel.SendMessageAsync($"Generating Elo Ratings for Game {gameId}...");

            await eloRatingService.GenerateEloRatingsForGameAsync(gameId);

            await Context.Channel.SendMessageAsync("Done.");
        }
    }
}
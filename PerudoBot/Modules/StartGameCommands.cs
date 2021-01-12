using Discord.Commands;
using PerudoBot.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("start")]
        public async Task Start()
        {
            if (await ValidateStateAsync(GameState.Setup) == false) return;

            var game = await GetGameAsync(GameState.Setup);

            ShufflePlayers(game);
            SetDice(game);

            var players = _perudoGameService.GetGamePlayers(game);

            game.State = (int)GameState.InProgress;
            game.PlayerTurnId = players.First().Id;
            game.RoundStartPlayerId = players.First().Id;

            await SendMessageAsync($"Starting the game!\nUse `!bid 2 2s` or `!exact` or `!liar` to play.");

            await RollDiceStartNewRoundAsync(game);

            _db.SaveChanges();
        }

        private void ShufflePlayers(Game game)
        {
            var players = _perudoGameService.GetGamePlayers(game);
            var r = new Random();
            var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();

            var turnOrder = 0;
            foreach (var player in shuffledPlayers)
            {
                player.TurnOrder = turnOrder;
                turnOrder += 1;
            }
            _db.SaveChanges();
        }

        private void SetDice(Game game)
        {
            var players = _perudoGameService.GetGamePlayers(game);

            foreach (var player in players)
            {
                if (game.PenaltyGainDice)
                {
                    player.NumberOfDice = 1;
                }
                else 
                { 
                    player.NumberOfDice = game.NumberOfDice;
                }
            }
            _db.SaveChanges();
        }
    }
}
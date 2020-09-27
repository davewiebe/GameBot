﻿using Discord.Commands;
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
            if (await ValidateState(GameState.Setup) == false) return;

            var game = GetGame(GameState.Setup);

            ShufflePlayers(game);
            SetDice(game);

            var players = GetPlayers(game);

            game.State = (int)GameState.InProgress;
            game.PlayerTurnId = players.First().Id;
            game.RoundStartPlayerId = players.First().Id;

            await SendMessage($"Starting the game!\nUse `!bid 2 2s` or `!exact` or `!liar` to play.");

            await RollDiceStartNewRound(game);

            _db.SaveChanges();
        }

        private void ShufflePlayers(Game game)
        {
            var players = GetPlayers(game);
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
            var players = GetPlayers(game);

            foreach (var player in players)
            {
                player.NumberOfDice = game.NumberOfDice;
            }
            _db.SaveChanges();
        }
    }
}
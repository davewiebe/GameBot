﻿using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task CheckGhostAttempts(Data.Game game)
        {
            var ghosts = GetPlayers(game)
                .Where(x => x.NumberOfDice == 0)
                .Where(x => x.GhostAttemptsLeft > 0).ToList();

            foreach (var ghost in ghosts)
            {
                var quantity = GetNumberOfDiceMatchingBid(game, ghost.GhostAttemptPips);

                if (quantity == ghost.GhostAttemptQuantity)
                {
                    ghost.GhostAttemptsLeft = -1;
                    ghost.NumberOfDice = 1;
                    ghost.Dice = "";
                    _db.SaveChanges();

                    await SendMessageAsync($":boom: A wild {GetUserNickname(ghost.Username)} appeared!");
                }
                else if (ghost.GhostAttemptQuantity > 0)
                {
                    ghost.GhostAttemptsLeft -= 1;
                    _db.SaveChanges();

                    if (ghost.GhostAttemptsLeft > 0)
                    {
                        await SendMessageAsync($":hourglass: {GetUserNickname(ghost.Username)} has {ghost.GhostAttemptsLeft} attempt left.");
                    }
                    else
                    {
                        await SendMessageAsync($":runner: {GetUserNickname(ghost.Username)} fled.");
                    }
                }
            }
        }
    }
}
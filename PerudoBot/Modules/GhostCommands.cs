using Discord;
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
                if (ghost.GhostAttemptPips == 0 || ghost.GhostAttemptQuantity == 0)
                {
                    ghost.GhostAttemptsLeft -= 1;
                    ghost.GhostAttemptQuantity = 0;
                    ghost.GhostAttemptPips = 0;
                    ghost.Dice = "";
                    _db.SaveChanges();

                    if (ghost.GhostAttemptsLeft > 0)
                    {
                        await SendMessageAsync($":hourglass: {ghost.Player.Nickname} has {ghost.GhostAttemptsLeft} attempt left.");
                    }
                    else
                    {
                        await SendMessageAsync($":runner: {ghost.Player.Nickname} fled.");
                    }

                    continue;
                }

                var quantity = GetNumberOfDiceMatchingBid(game, ghost.GhostAttemptPips);

                if (quantity == ghost.GhostAttemptQuantity)
                {
                    ghost.GhostAttemptsLeft = -1;
                    ghost.NumberOfDice = 1;
                    ghost.GhostAttemptQuantity = 0;
                    ghost.GhostAttemptPips = 0;
                    ghost.Dice = "";
                    _db.SaveChanges();

                    await SendMessageAsync($":boom: A wild {ghost.Player.Nickname} appeared!");
                }
                else if (ghost.GhostAttemptQuantity > 0)
                {
                    ghost.GhostAttemptsLeft -= 1;
                    ghost.GhostAttemptQuantity = 0;
                    ghost.GhostAttemptPips = 0;
                    _db.SaveChanges();

                    if (ghost.GhostAttemptsLeft > 0)
                    {
                        await SendMessageAsync($":hourglass: {ghost.Player.Nickname} has {ghost.GhostAttemptsLeft} attempt left.");
                    }
                    else
                    {
                        await SendMessageAsync($":runner: {ghost.Player.Nickname} fled.");
                    }
                }
            }
        }
    }
}
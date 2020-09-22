using Discord.Commands;
using Discord.WebSocket;
using Npgsql.TypeHandlers.DateTimeHandlers;
using PerudoBot.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        public async Task KickUserFromGame(params string[] stringArray)
        {
            var game = GetGame(IN_PROGRESS);

            var userToKick = Context.Message.MentionedUsers.Single();
            var player = GetPlayers(game).Where(x => x.Username == userToKick.Username).Single();

            int monkey = 0;
            if (int.TryParse(stringArray[0], out monkey))
            {
                if (game.Id + player.Id == monkey)
                {
                    player.NumberOfDice = 0;
                    await SendMessage($"{GetUserNickname(userToKick.Username)} has been kicked.");

                    await SendRoundSummaryForBots(game);
                    await GetRoundSummary(game);

                    SetTurnPlayerToRoundStartPlayer(game);
                    Thread.Sleep(2000);
                    await RollDiceStartNewRound(game);
                    return;
                }
                return;
            }

            await SendMessage($"{GetUserNickname(userToKick.Username)} has been kicked.");

            Thread.Sleep(6000);


            await SendMessage($"LOL JK.");
            Thread.Sleep(2000);

            await SendMessage($"But if you do want to kick them, send `!kick {game.Id + player.Id} @{player.Username}`");
        }
    }
}

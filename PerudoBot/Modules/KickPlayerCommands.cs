using Discord.Commands;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("kick", RunMode = RunMode.Async)]
        public async Task KickUserFromGame(params string[] stringArray)
        {
            var game = await GetGameAsync(GameState.InProgress);

            var userToKick = Context.Message.MentionedUsers.Single();
            var player = GetGamePlayers(game).Where(x => x.NumberOfDice > 0).Where(x => x.Player.Username == userToKick.Username).Single();

            int monkey = 0;
            if (int.TryParse(stringArray[0], out monkey))
            {
                if (game.Id + player.Id == monkey)
                {
                    player.NumberOfDice = 0;
                    await SendMessageAsync($"{GetUserNickname(userToKick.Username)} has been kicked.");

                    await SendRoundSummaryForBots(game);
                    await SendRoundSummary(game);

                    SetTurnPlayerToRoundStartPlayer(game);
                    Thread.Sleep(2000);
                    await RollDiceStartNewRound(game);
                    return;
                }
                return;
            }

            await SendMessageAsync($"{GetUserNickname(userToKick.Username)} has been kicked.");

            Thread.Sleep(6000);

            await SendMessageAsync($"LOL JK.");
            Thread.Sleep(2000);

            await SendMessageAsync($"But if you do want to kick them, send `!kick {game.Id + player.Id} @{player.Player.Nickname}`");
        }
    }
}
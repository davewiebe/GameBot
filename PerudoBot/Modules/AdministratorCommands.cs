using Discord.Commands;
using PerudoBot.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PerudoBot.Extensions;
using Discord;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("updateplayerdicejoshdontabusethis")]
        public async Task UpdatePlayerDice(params string[] stringArray)
        {
            var game = await GetGameAsync(GameState.InProgress);

            var userToAddDiceTo = Context.Message.MentionedUsers.Single();
            var player = _perudoGameService.GetGamePlayers(game).Where(x => x.Player.Username == userToAddDiceTo.Username).Single();

            int monkey = 0;
            if (int.TryParse(stringArray[0], out monkey))
            {
                player.NumberOfDice = monkey;
                await SendMessageAsync($"{GetUserNickname(userToAddDiceTo.Username)}'s dice has been updated to {monkey}.");

                await SendRoundSummaryForBots(game);
                await SendRoundSummary(game);

                SetTurnPlayerToRoundStartPlayer(game);
                Thread.Sleep(2000);
                await RollDiceStartNewRoundAsync(game);
                return;
            }
        }

        [Command("resetroundjoshdontabusethis")]
        public async Task ResetRound(params string[] stringArray)
        {
            var game = await GetGameAsync(GameState.InProgress);

            await SendMessageAsync($"ROUND RESET TRIGGERED");

            await RollDiceStartNewRoundAsync(game);
        }
        [Command("resenddice")]
        public async Task ResendDice(params string[] stringArray)
        {

            var game = await GetGameAsync(GameState.InProgress);
            var gamePlayers = _perudoGameService.GetGamePlayers(game);

            var activeGamePlayers = gamePlayers.Where(x => x.NumberOfDice > 0);

            foreach (var gamePlayer in activeGamePlayers)
            {

                var user = Context.Guild.Users.Single(x => x.Username == gamePlayer.Player.Username);
                var message = $"Your dice: {string.Join(" ", gamePlayer.Dice.Split(",").Select(x => int.Parse(x).GetEmoji()))}";

                var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
                await user.SendMessageAsync(message, options: requestOptions);
            }

            await _db.SaveChangesAsync();
        }
    }
}
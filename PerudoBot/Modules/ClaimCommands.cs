using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Claims;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("claim")]
        public async Task ClaimDiceAsync([Remainder] string claimText)
        {
            if (claimText.Contains("help"))
            {
                await ClaimHelpAsync();
                return;
            }

            claimText = claimText.Replace("❤️", "<3");

            DeleteCommandFromDiscord(Context.Message.Id);

            var game = await GetGameAsync(GameState.InProgress);

            var gamePlayer = game.GamePlayers
                    .Where(x => x.NumberOfDice > 0)
                    .Where(x => x.Player.Username == Context.User.Username)
                    .SingleOrDefault();

            if (gamePlayer == null)
                return;

            Claim claim;
            try
            {
                claim = ClaimParser.Parse(claimText);

                var message = await SendMessageAsync($":loudspeaker: {gamePlayer.Player.Nickname} claims " +
                    $"{claim.Operator.ToReadableString()} `{claim.Quantity}` x {claim.Pips.GetEmoji()}, {(claim.IncludeWilds ? "" : "(no wilds)")}...");

                Thread.Sleep(2000);

                var isClaimValid = ClaimValidator.Validate(claim, gamePlayer.Dice);

                var validationMessage = isClaimValid ? ":white_check_mark: It's Legit!" : ":x: Nope!";

                await message.ModifyAsync(msg => msg.Content = $"{message.Content} {validationMessage}");
            }
            catch (ArgumentException argumentException)
            {
                await SendMessageAsync(argumentException.Message);
            }
        }

        [Command("claimhelp")]
        public async Task ClaimHelpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Claim Help")
                .AddField("Claim Exact Count, no wilds", "[quantity] [pips] => 2 4")
                .AddField("Claim Greater Than, including wilds", ">[quantity] [pips]* ex. >2 4*")
                .AddField("Claim Less Than, including wilds", "<[quantity] [pips]* ex. <2 4*")
                .AddField("Claim Approximate (+/-1), no wilds", "~[quantity] [pips] ex. ~2 4")
                .Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }
    }
}
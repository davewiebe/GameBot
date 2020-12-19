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
        [Alias("c")]
        public async Task ClaimDiceAsync([Remainder] string claimText)
        {
            //await SendMessageAsync("I thought you wanted to be on the nice list?");
            return;

            if (claimText.Contains("help"))
            {
                await ClaimHelpAsync();
                return;
            }

            claimText = claimText.Replace("❤️", "<3");

            var game = await GetGameAsync(GameState.InProgress);

            var gamePlayer = game.GamePlayers
                    .Where(x => x.NumberOfDice > 0)
                    .Where(x => x.Player.Username == Context.User.Username)
                    .SingleOrDefault();

            if (gamePlayer == null)
                return;

            // make sure claim calls from previous rounds don't carry over if called too late
            if (Context.Message.Timestamp.ToLocalTime() < game.CurrentRound.DateStarted)
            {
                return;
            }

            if (!(game.CurrentRound is StandardRound))
            {
                await SendMessageAsync("Sorry, claims are only currently supported in standard rounds.");
                return;
            }

            Claim claim;
            try
            {
                claim = ClaimParser.Parse(claimText);

                DeleteCommandFromDiscord(Context.Message.Id);

                var message = await SendMessageAsync($":loudspeaker: {gamePlayer.Player.Nickname} claims " +
                    $"{claim.Operator.ToReadableString()} `{claim.Quantity}` x {claim.Pips.GetEmoji()}{(claim.IncludeWilds ? "" : " (no wilds)")} ...");

                Thread.Sleep(2000);

                var isClaimValid = ClaimValidator.Validate(claim, gamePlayer.Dice);

                var validationMessage = isClaimValid ? ":white_check_mark: It's Legit!" : ":x: Nope!";

                await message.ModifyAsync(msg => msg.Content = $"{message.Content} {validationMessage}");
            }
            catch (ArgumentException)
            {
                await SendMessageAsync($"Sorry, your claim doesn't jive. Try `!claim help` for help.");
            }
        }

        [Command("claimhelp")]
        public async Task ClaimHelpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle(":loudspeaker: Claim Help")
                .WithDescription("Claims are the hot new feature that allow you claim something about your hand that can be " +
                "verified by the Perudo game bot.\nClaims follow the same syntax structure as bids and are called with `!claim` or `!c`, " +
                "with the following options:")
                .AddField("Claim Exact", "`!claim [quantity] [pips]` ex. `!claim 2 4`")
                .AddField("Claim More Than (`>`)", "`!claim >[quantity] [pips]` ex. `!claim >2 4`")
                .AddField("Claim Less Than (`<`)", "`!claim <[quantity] [pips]` ex. `!claim <2 4`")
                .AddField("Claim Approximate (+/-1) (`~`)", "`!c ~[quantity] [pips]` ex. `!c ~2 4`")
                .AddField("Claim Excluding Wilds (`!`)", "`!c [quantity] [pips]!` ex. `!c 2 4!`")
                .Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }
    }
}
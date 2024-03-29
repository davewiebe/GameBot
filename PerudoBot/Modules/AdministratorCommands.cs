﻿using Discord.Commands;
using PerudoBot.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PerudoBot.Extensions;
using Discord;
using Discord.WebSocket;

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

        [Command("mahtg", RunMode =RunMode.Async)]
        public async Task Mahtg(params string[] stringArray)
        {
            await Context.Message.AddReactionAsync(new Emoji("Ⓜ️"));
            await Context.Message.AddReactionAsync(new Emoji("🅰"));
            await Context.Message.AddReactionAsync(new Emoji("🇭"));
            await Context.Message.AddReactionAsync(new Emoji("🇹"));
            await Context.Message.AddReactionAsync(new Emoji("🇬"));
        }

        [Command("hot", RunMode = RunMode.Async)]
        [Alias("bigcat", "cat")]
        public async Task hot(params string[] stringArray)
        {
            await SendCustomEmojAsync("BIGCAT");
        }

        [Command("rtr", RunMode = RunMode.Async)]
        public async Task Rtr(params string[] stringArray)
        {
            await SendCustomEmojAsync("ezgif29c70deda4b1c");
        }

        [Command("monkaS", RunMode = RunMode.Async)]
        public async Task monkas(params string[] stringArray)
        {
            await SendCustomEmojAsync("monkaS");
        }
        [Command("poggers", RunMode = RunMode.Async)]
        public async Task poggers(params string[] stringArray)
        {
            await SendCustomEmojAsync("poggers");
        }
        [Command("feelsbadman", RunMode = RunMode.Async)]
        public async Task feelsbadman(params string[] stringArray)
        {
            await SendCustomEmojAsync("FeelsBadMan");
        }

        [Command("kekw", RunMode = RunMode.Async)]
        public async Task kekw(params string[] stringArray)
        {
            await SendCustomEmojAsync("kekw");
        }
        [Command("spook")]
        public async Task spook(params string[] stringArray)
        {
            await SendCustomEmojAsync("spook");
        }
        [Command("nah")]
        public async Task nah(params string[] stringArray)
        {
            await SendCustomEmojAsync("nah");
        }


        [Command("r")]
        public async Task r(string emoji)
        {
            DeleteCommandFromDiscord();

            if (emoji.Contains(":"))
            {
                emoji = emoji.Split(":")[1];
            }

            IEmote emote = Context.Guild.Emotes.FirstOrDefault(e => e.Name == emoji);

            if (emote == null)
            {
                await SendEmojAsync(new Emoji(emoji));
                return;
            }

            // Reacts to the message with the Emote.
            var game = await GetGameAsync(GameState.InProgress);


            var lastBid = GetMostRecentBid(game);
            if (lastBid == null) return;
            if (lastBid.Quantity == 0) return;

            var lastBidMessage = await Context.Channel.GetMessageAsync(lastBid.MessageId);

            await lastBidMessage.AddReactionAsync(emote);
        }

        [Command("otg")]
        public async Task otg(params string[] stringArray)
        {
            await SendCustomEmojAsync("otg");
        }
        [Command("sus")]
        public async Task sus(params string[] stringArray)
        {
            await SendCustomEmojAsync("sus");
        }

        [Command("fire")]
        public async Task fire(params string[] stringArray)
        {
            await SendEmojAsync(new Emoji("🔥"));
        }

        [Command("ice")]

        public async Task ice(params string[] stringArray)
        {
            await SendEmojAsync(new Emoji("🧊"));
        }
        [Command("baloon")]
        [Alias("bal", "lb", "load", "baloons", "lob", "loadofbaloons")]
        public async Task lob(params string[] stringArray)
        {
            await SendCustomEmojAsync("lob");
        }

        [Command("vpn")]
        public async Task vpn(params string[] stringArray)
        {
            IEmote up = Context.Guild.Emotes.First(e => e.Name == "vpn");
            IEmote down = Context.Guild.Emotes.First(e => e.Name == "novpn");
            await Context.Message.AddReactionsAsync(new[] { up, down});
        }

        [Command("vote")]
        public async Task vote(params string[] stringArray)
        {
            await Context.Message.AddReactionsAsync(new[] { new Emoji("👍"), new Emoji("👎") });
        }

        private async Task SendCustomEmojAsync(string v)
        {
            DeleteCommandFromDiscord();

            IEmote emote = Context.Guild.Emotes.First(e => e.Name == v);

            // Reacts to the message with the Emote.
            var game = await GetGameAsync(GameState.InProgress);


            var lastBid = GetMostRecentBid(game);
            if (lastBid == null) return;
            if (lastBid.Quantity == 0) return;

            var lastBidMessage = await Context.Channel.GetMessageAsync(lastBid.MessageId);

            await lastBidMessage.AddReactionAsync(emote);
        }

        private async Task SendEmojAsync(IEmote emote)
        {
            DeleteCommandFromDiscord();

            // Reacts to the message with the Emote.
            var game = await GetGameAsync(GameState.InProgress);


            var lastBid = GetMostRecentBid(game);
            if (lastBid == null) return;
            if (lastBid.Quantity == 0) return;

            var lastBidMessage = await Context.Channel.GetMessageAsync(lastBid.MessageId);

            await lastBidMessage.AddReactionAsync(emote);
        }
    }
}
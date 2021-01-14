using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Claims;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("deal")]
        [Alias("d")]
        public async Task Deal(params string[] x)
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;

            var game = await GetGameAsync(GameState.InProgress);


            var activePlayer = GetActivePlayer(game);

            if (activePlayer.Player.UserId != Context.User.Id) return;

            RemoveActiveDeals(game);
            activePlayer.HasActiveDeal = true;

            _db.SaveChanges();

            DeleteCommandFromDiscord();
            await SendMessageAsync($":money_with_wings: {activePlayer.Player.Nickname} wants to make a deal");
        }

        [Command("payup")]
        public async Task Payup(params string[] x)
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;
            var game = await GetGameAsync(GameState.InProgress);
            var activePlayer = GetActivePlayer(game);
            var mentionedUser = _perudoGameService
                .GetGamePlayers(game)
                .Where(x => x.NumberOfDice > 0)
                .Single(x => x.Player.UserId == Context.Message.MentionedUsers.First().Id);

            if (!activePlayer.UserDealIds.Contains(mentionedUser.Id.ToString())) return;

            var activeDeals = activePlayer.UserDealIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            activeDeals.Remove(mentionedUser.Id.ToString());
            activePlayer.UserDealIds = string.Join(',', activeDeals);


            DeleteCommandFromDiscord();

            await SendMessageAsync($":moneybag: Alright {GetUser(mentionedUser.Player.Username).Mention}, time to pay up. " +
                $"Take {activePlayer.Player.Nickname}'s turn for them.");

            game.DealCurrentGamePlayerId = mentionedUser.Id;

            RemoveActiveDeals(game);
            _db.SaveChanges();
        }

        [Command("deals")]
        [Alias("debts")]
        public async Task PrintDeals(params string[] x)
        {
            if (await ValidateStateAsync(GameState.InProgress) == false) return;
            var game = await GetGameAsync(GameState.InProgress);

            DeleteCommandFromDiscord();
            var deals = new List<string>();

            var activePlayers = game.GamePlayers.Where(x => x.NumberOfDice > 0).ToList();
            foreach (var player in activePlayers)
            {
                if (player.UserDealIds == null) continue;
                var userDeals = player.UserDealIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (userDeals.Length == 0) continue;

                var dealText = $":moneybag: {player.Player.Nickname} is owed by: ";

                foreach (var deal in userDeals)
                {
                    if (deal == "") continue;
                    var user = game.GamePlayers.SingleOrDefault(x => x.Id == int.Parse(deal) && x.NumberOfDice > 0);

                    if (user == null) continue;

                    dealText += $":money_with_wings: {user.Player.Nickname} ";
                }
                deals.Add(dealText);
            }

            var pendingDeals = new List<string>();

            foreach (var player in activePlayers)
            {
                if (player.PendingUserDealIds == null) continue;
                var userDeals = player.PendingUserDealIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (userDeals.Length == 0) continue;

                var dealText = $":moneybag: {player.Player.Nickname} will be owed by: ";

                foreach (var deal in userDeals)
                {
                    if (deal == "") continue;
                    var user = game.GamePlayers.SingleOrDefault(x => x.Id == int.Parse(deal) && x.NumberOfDice > 0);

                    if (user == null) continue;

                    dealText += $":money_with_wings: {user.Player.Nickname} ";
                }
                pendingDeals.Add(dealText);
            }

            var builder = new EmbedBuilder()
                .WithTitle(":money_with_wings: Debts :money_with_wings:")
                .AddField("Current debts", $"{string.Join("\n", deals)}", inline: false)
                .AddField("Pending debts", $"{string.Join("\n", pendingDeals)}", inline: false);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}
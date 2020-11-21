using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("gamelog")]
        public async Task Gamelogs(params string[] stringArray)
        {
            var page = 1;
            var i = -1;
            if (stringArray.Length == 1)
            {
                if (stringArray[0].ToLower().StartsWith("pg"))
                {
                    page = int.Parse(stringArray[0].Substring(2));
                }
                else
                {
                    i = int.Parse(stringArray[0]);
                }
            }

            var gamelogService = new GamelogService(_db);
            var embedString = gamelogService.GetGamelog(Context.Guild.Id, page, i);

            var guildUsers = Context.Guild.Users;
            foreach (var guildUser in guildUsers)
            {
                if (guildUser.Nickname == null) continue;
                embedString = embedString.Replace(guildUser.Username, guildUser.Nickname);
            }

            var pageText = i == -1 ? $" - Page {page}" : "";

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs{pageText}")
                                .AddField("Games", embedString, inline: false);
            var embed = builder.Build();

            var message = await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            if (i != -1) return;

            if (page != 1)
            {
                await message.AddReactionAsync(new Emoji("⬅️"));
            }
            if (!embedString.Contains("01."))
            {
                _ = message.AddReactionAsync(new Emoji("➡️"));
            }
        }

        [Command("gamelogs")]
        public async Task Gamelog(params string[] stringArray)
        {
            await Gamelogs(stringArray);
        }

        [Command("leaderboard")]
        [Alias("lb")]
        public async Task Leaderboard(params string[] parameters)
        {
            var options = parameters.Select(o => o.ToLower());
            var gameMode = "All Ranked Games";

            var baseQuery = _db.GamePlayers.AsQueryable().AsNoTracking();

            if (options.Any(o => o == "suddendeath"))
            {
                gameMode = "Sudden Death";
                baseQuery = baseQuery.Where(p => p.Game.Penalty == 100);
            }

            if (options.Any(o => o == "variable"))
            {
                gameMode = "Variable Penalty";
                baseQuery = baseQuery.Where(p => p.Game.Penalty == 0);
            }

            if (options.Any(o => o == "standard"))
            {
                gameMode = "Standard Penalty";
                baseQuery = baseQuery.Where(p => p.Game.Penalty == 1);
            }

            var result = baseQuery.Where(p => p.Game.IsRanked)
                .Where(p => p.Game.State == 3)
                .Where(p => p.Game.GuildId == Context.Guild.Id)
                .Where(p => !p.Player.IsBot)
                .Select(p => new
                {
                    GameId = p.Game.Id,
                    p.Game.IsRanked,
                    p.Game.State,
                    p.Game.DateFinished,
                    p.Player.Username,
                    p.Player.Nickname,
                    // TODO: User GameUserId instead, when winner points to GamePlayerId
                    IsWinner = (p.Player.Username == p.Game.Winner
                        && p.Player.GuildId == p.Game.GuildId) ? 1 : 0,
                    PlayerCount = p.Game.GamePlayers.Count()
                })
                .Where(p => p.PlayerCount >= 3 && p.PlayerCount <= 100)
                .OrderByDescending(p => p.DateFinished)
                .GroupBy(g => new { g.Username, g.Nickname })
                .Select(g => new
                {
                    g.Key.Username,
                    g.Key.Nickname,
                    GamesPlayed = g.Count(),
                    Wins = g.Sum(x => x.IsWinner),
                    WinPercentage = ((double)g.Sum(x => x.IsWinner) / (double)g.Count()) * 100
                })
                .OrderByDescending(f => f.WinPercentage)
                .ThenByDescending(f => f.GamesPlayed);

            var usernamePadding = 13;
            var gamesPlayedPadding = 5;
            var winsPadding = 7;
            var winPercentagePadding = 8;

            var embedString = "Username".PadLeft(usernamePadding) + "GP".PadLeft(gamesPlayedPadding)
                + "Wins".PadLeft(winsPadding) + "Win %".PadLeft(winPercentagePadding) + "\n";

            foreach (var item in result)
            {
                var username = item.Nickname.PadLeft(usernamePadding);
                var gamesPlayed = item.GamesPlayed.ToString().PadLeft(gamesPlayedPadding);
                var wins = item.Wins.ToString().PadLeft(winsPadding);
                var winPercentage = item.WinPercentage.ToString("0.0").PadLeft(winPercentagePadding);

                embedString += $"{username}{gamesPlayed}{wins}{winPercentage}\n";
            }

            var builder = new EmbedBuilder()
                                .WithTitle($"Leaderboard")
                                .AddField(gameMode, $"```{embedString}```", inline: false)
                                .AddField("Player count", "4+");
            var embed = builder.Build();

            _ = await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("awards")]
        [Alias("records, walloffame")]
        public async Task AwardsAsync(params string[] parameters)
        {
            await Context.Channel.TriggerTypingAsync();
            var statsService = new StatsService(_db);

            var builder = new EmbedBuilder()
                    .WithTitle($"Wall of Fame");

            var topActionCountsInAGame = statsService.GetTopActionsInAGame(Context.Guild.Id, 1);

            var mostCorrectLiarCalls = topActionCountsInAGame
                .Where(t => t.IsSuccess)
                .Where(t => t.ActionType == nameof(LiarCall))
                .Where(t => !t.IsOutOfTurn)
                .FirstOrDefault();

            var mostCorrectLiarCallsOutOfTurn = topActionCountsInAGame
                .Where(t => t.IsSuccess)
                .Where(t => t.ActionType == nameof(LiarCall))
                .Where(t => t.IsOutOfTurn)
                .FirstOrDefault();

            var mostCorrectExactCalls = topActionCountsInAGame
                .Where(t => t.IsSuccess)
                .Where(t => t.ActionType == nameof(ExactCall))
                .Where(t => !t.IsOutOfTurn)
                .FirstOrDefault();

            var mostBids = topActionCountsInAGame
                .Where(t => t.IsSuccess)
                .Where(t => t.ActionType == nameof(Bid))
                .FirstOrDefault();

            builder.AddField($":lying_face: Most Correct Liar Calls",
                (mostCorrectLiarCalls != null ? mostCorrectLiarCalls.TopRecords.ToStringWithNewlines()
                    : "No records").WrapInCodeBlock());

            builder.AddField($":lying_face::twisted_rightwards_arrows: Most Correct Liar Calls (Out Of Turn)",
                (mostCorrectLiarCallsOutOfTurn != null ? mostCorrectLiarCallsOutOfTurn.TopRecords.ToStringWithNewlines()
                    : "No records").WrapInCodeBlock());

            builder.AddField($":dart: Most Correct Exact Calls",
                (mostCorrectExactCalls != null ? mostCorrectExactCalls.TopRecords.ToStringWithNewlines()
                    : "No records").WrapInCodeBlock());

            builder.AddField($":tickets: Most Bids",
                (mostBids != null ? mostBids.TopRecords.ToStringWithNewlines()
                    : "No records").WrapInCodeBlock());

            _ = await Context.Channel.SendMessageAsync(null, embed: builder.Build()).ConfigureAwait(false);
        }
    }
}
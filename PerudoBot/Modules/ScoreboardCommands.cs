using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
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
            var gameMode = "All Games";

            var baseQuery = _db.Players.AsQueryable().AsNoTracking();

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
                .Where(p => !p.IsBot)
                .Select(p => new
                {
                    GameId = p.Game.Id,
                    p.Game.IsRanked,
                    p.Game.State,
                    p.Username,
                    IsWinner = (p.Username == p.Game.Winner) ? 1 : 0,
                    PlayerCount = p.Game.Players.Count()
                })
                .Where(p => p.PlayerCount >= 3 && p.PlayerCount <= 100)
                .GroupBy(g => g.Username)
                .Select(g => new
                {
                    Username = g.Key,
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
                var guildUser = Context.Guild.Users
                    .FirstOrDefault(u => u.Username == item.Username);

                var username = "";
                if (guildUser == null)
                {
                    username = item.Username;
                }
                else
                {
                    username = (guildUser.Nickname != null ? guildUser.Nickname : item.Username);
                }

                username = username.PadLeft(usernamePadding);

                var gamesPlayed = item.GamesPlayed.ToString().PadLeft(gamesPlayedPadding);
                var wins = item.Wins.ToString().PadLeft(winsPadding);
                var winPercentage = item.WinPercentage.ToString("0.0").PadLeft(winPercentagePadding);

                embedString += $"{username}{gamesPlayed}{wins}{winPercentage}\n";
            }

            var builder = new EmbedBuilder()
                                .WithTitle($"Leaderboard")
                                .AddField(gameMode, $"```{embedString}```", inline: false);
            var embed = builder.Build();

            _ = await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
    }
}
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
        public async Task Leaderboard(params string[] stringArray)
        {
            await SendMessageAsync("`!leaderboard` has been depricated. Try `!gamelogs`");
        }
    }
}
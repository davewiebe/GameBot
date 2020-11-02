using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using PerudoBot.Services;
using Game = PerudoBot.Data.Game;
using System;
using Microsoft.EntityFrameworkCore;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands : ModuleBase<CommandContext>
    {
        private readonly GameBotDbContext _db;
        private readonly PerudoGameService _perudoGameService;

        public ReactionCommands()
        {
            //TODO: Let DI handle instantiation
            _db = new GameBotDbContext();
            _perudoGameService = new PerudoGameService(_db);
        }

        [Command("➡️")]
        public async Task Next()
        {
            var message = Context.Message;

            int currentPage = GetCurrentPage(message);

            var newPage = currentPage + 1;

            var gamelogService = new GamelogService(_db);

            var embedString = gamelogService.GetGamelog(Context.Guild.Id, newPage, -1);

            var guildUsers = await Context.Guild.GetUsersAsync();
            foreach (var guildUser in guildUsers)
            {
                if (guildUser.Nickname == null) continue;
                embedString = embedString.Replace(guildUser.Username, guildUser.Nickname);
            }

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .AddField("Games", embedString, inline: false);
            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await Context.Message.ModifyAsync(x => x.Embed = embed);

            if (newPage != 1)
            {
                await message.AddReactionAsync(new Emoji("⬅️"));
            }
            if (!embedString.Contains("01."))
            {
                _ = message.AddReactionAsync(new Emoji("➡️"));
            }
        }

        private static int GetCurrentPage(IUserMessage message)
        {
            var currentPage = 1;
            if (message.Embeds.First().Title.Contains("Page"))
            {
                var pageText = message.Embeds.First().Title.Split("Page")[1];
                currentPage = int.Parse(pageText);
            }

            return currentPage;
        }

        [Command("⬅️")]
        public async Task Prev()
        {
            var message = Context.Message;
            int currentPage = GetCurrentPage(message);

            var newPage = currentPage - 1;
            if (newPage == 0) return;

            var gamelogService = new GamelogService(_db);

            var embedString = gamelogService.GetGamelog(Context.Guild.Id, newPage, -1);

            var guildUsers = await Context.Guild.GetUsersAsync();
            foreach (var guildUser in guildUsers)
            {
                if (guildUser.Nickname == null) continue;
                embedString = embedString.Replace(guildUser.Username, guildUser.Nickname);
            }

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .AddField("Games", embedString, inline: false);
            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await Context.Message.ModifyAsync(x => x.Embed = embed);

            if (newPage != 1)
            {
                await message.AddReactionAsync(new Emoji("⬅️"));
            }

            if (!embedString.Contains("01."))
            {
                _ = message.AddReactionAsync(new Emoji("➡️"));
            }
        }
    }

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly GameBotDbContext _db;
        private readonly PerudoGameService _perudoGameService;

        public Commands()
        {
            //TODO: Let DI handle instantiation
            _db = new GameBotDbContext();
            _perudoGameService = new PerudoGameService(_db);
        }

        private async Task<IUserMessage> SendMessageAsync(string message, bool isTTS = false)
        {
            if (string.IsNullOrEmpty(message)) return null;

            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            return await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
        }

        private async Task SendTempMessageAsync(string message, bool isTTS = false)
        {
            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            var sentMessage = await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
            try
            {
                _ = sentMessage.DeleteAsync();
            }
            catch
            { }
        }

        private async Task<bool> ValidateStateAsync(GameState gameState)
        {
            var game = await GetGameAsync(gameState);

            if (game == null)
            {
                await SendMessageAsync($"Cannot do that at this time.");
                return false;
            }
            return true;
        }

        private async Task<Game> GetGameAsync(params GameState[] gameStates)
        {
            return await _perudoGameService.GetGameAsync(Context.Channel.Id, gameStates);
        }

        private void DeleteCommandFromDiscord(ulong? messageId = null)
        {
            try
            {
                if (messageId != null)
                {
                    _ = Task.Run(() => Context.Channel.DeleteMessageAsync(messageId.Value));
                }
                else
                {
                    _ = Task.Run(() => Context.Message.DeleteAsync());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
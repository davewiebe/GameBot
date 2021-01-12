using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using PerudoBot.Services;

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

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .WithDescription(embedString);
            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await Context.Message.ModifyAsync(x => x.Embed = embed);

            if (newPage != 1)
            {
                await message.AddReactionAsync(new Emoji("⬅️"));
            }
            if (!embedString.Contains("01."))
            { }
            await message.AddReactionAsync(new Emoji("➡️"));
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

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .WithDescription(embedString);

            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await Context.Message.ModifyAsync(x => x.Embed = embed);

            if (newPage != 1)
            {
                await message.AddReactionAsync(new Emoji("⬅️"));
            }

            if (!embedString.Contains("01."))
            { }
            await message.AddReactionAsync(new Emoji("➡️"));
        }
    }
}
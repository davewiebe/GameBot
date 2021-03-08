using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using PerudoBot.Services;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands// : ModuleBase<CommandContext>
    {
        private readonly GameBotDbContext _db;
        private readonly PerudoGameService _perudoGameService;

        public ReactionCommands(GameBotDbContext db)
        {
            _db = db;
            _perudoGameService = new PerudoGameService(_db);
        }

        public async Task ReactToThis(CommandContext context, ulong userid, string emoji)
        {
            switch (emoji)
            {
                case "➡️": 
                    await Next(context);
                    break;
                case "⬅️":
                    await Prev(context);
                    break;
                case "🔥":
                    await DiceOption(context);
                    break;
                case "🪓": // axe
                    await DiceOption(context);
                    break;
                case "➕":
                    await AddPlayer(context, userid);
                    break;
                case "➖":
                    await RemovePlayer(context, userid);
                    break;
                default:
                    break;
            }
        }

        //[Command("➡️")]
        public async Task Next(CommandContext context)
        {
            var message = context.Message;

            int currentPage = GetCurrentPage(message);

            var newPage = currentPage + 1;

            var gamelogService = new GamelogService(_db);

            var embedString = gamelogService.GetGamelog(context.Guild.Id, newPage, -1);

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .WithDescription(embedString);
            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await context.Message.ModifyAsync(x => x.Embed = embed);

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

        //[Command("⬅️")]
        public async Task Prev(CommandContext context)
        {
            var message = context.Message;
            int currentPage = GetCurrentPage(message);

            var newPage = currentPage - 1;
            if (newPage == 0) return;

            var gamelogService = new GamelogService(_db);

            var embedString = gamelogService.GetGamelog(context.Guild.Id, newPage, -1);

            var builder = new EmbedBuilder()
                                .WithTitle($"Game logs - Page {newPage}")
                                .WithDescription(embedString);

            var embed = builder.Build();

            await message.RemoveAllReactionsAsync();
            await context.Message.ModifyAsync(x => x.Embed = embed);

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
using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;
using PerudoBot.Services;
using Game = PerudoBot.Data.Game;
using System;

namespace PerudoBot.Modules
{
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

        private async Task SendMessage(string message, bool isTTS = false)
        {
            if (string.IsNullOrEmpty(message)) return;

            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
        }

        private async Task SendTempMessage(string message, bool isTTS = false)
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

        private async Task<bool> ValidateState(GameState gameState)
        {
            var game = GetGame(gameState);

            if (game == null)
            {
                await SendMessage($"Cannot do that at this time.");
                return false;
            }
            return true;
        }

        private Game GetGame(params GameState[] gameStates)
        {
            var gameStateIds = gameStates.Cast<int>().ToList();

            return _db.Games.AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .Where(x => gameStateIds.Contains(x.State))
                .SingleOrDefault();
        }

        private void RemoveUserCommand()
        {
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
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
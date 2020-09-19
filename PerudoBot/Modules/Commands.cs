using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int SETUP = 0;
        private const int IN_PROGRESS = 1;
        private const int TERMINATED = 2;
        private const int FINISHED = 3;

        private GameBotDbContext _db;

        public Commands()
        {
            _db = new GameBotDbContext();
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

        private async Task<bool> ValidateState(int stateId)
        {
            var game = GetGame(stateId);

            if (game == null)
            {
                await SendMessage($"Cannot do that at this time.");
                return false;
            }
            return true;
        }

        private Data.Game GetGame(int stateId)
        {
            return _db.Games.AsQueryable()
                .Where(x => x.ChannelId == Context.Channel.Id)
                .SingleOrDefault(x => x.State == stateId);
        }
    }
}

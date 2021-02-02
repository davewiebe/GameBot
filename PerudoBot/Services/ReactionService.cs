using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using PerudoBot.Data;
using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class ReactionService : IDisposable
    {
        public ReactionService()
        {
        }

        //public Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception);
        public async Task ExecuteAsync(CommandContext context, ulong userid, string emoji, IServiceProvider services)
        {
            var serviceScopeFactory = (IServiceScopeFactory)services.GetService(typeof(IServiceScopeFactory));

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var services2 = scope.ServiceProvider;
                var _db = services.GetRequiredService<GameBotDbContext>();

                var monkey = new ReactionCommands(_db);
                await monkey.ReactToThis(context, userid, emoji);
            }
        }

        public void Dispose()
        {
        }
    }
}

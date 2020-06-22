using Discord.Commands;
using Discord.WebSocket;
using GameBot.Enums;
using GameBot.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("hecklecoffeebot")]
        public async Task HeckleCoffeeBot()
        {
            if (!Context.Message.Author.IsBot) return;

            Console.WriteLine(Context.Message.Author.Username);
            Console.WriteLine(Context.Message.Content);
            Console.WriteLine(Context.Message.Timestamp);

            var userMention = Context.Message.Author.Mention;
            _phraseService.AddReplacement("<coffeebot>", userMention);
            
            if (Context.Message.Timestamp.Hour == 21 && Context.Message.Timestamp.Minute == 0)
            {
                await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.HeckleCoffeeBot));
            }
        }
    }
}

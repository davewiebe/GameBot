using Discord;
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
        [Command("heckle")]
        public async Task Heckle()
        {
            if (Context.Message.Author.Username != "dawiebe") return;
            await ReplyAsync("coffee remindme 5m to burn you");
        }


        [Command("hecklecoffeebot")]
        public async Task HeckleCoffeeBot()
        {
            if (!Context.Message.Author.IsBot) return;

            Console.WriteLine(Context.Message.Timestamp);
            Console.WriteLine(Context.Message.Author.Username);
            Console.WriteLine(Context.Message.Content);

            var userMention = Context.Message.Author.Mention;
            _phraseService.AddReplacement("<coffeebot>", userMention);

            if (Context.Message.Author.Username == "CoffeeBot" && Context.Message.Embeds.Count > 0)
            {
                var embed = Context.Message.Embeds.First();
                if (embed.Title.Contains("Coffee Alert"))
                {
                    //await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.HeckleCoffeeBot));
                    await ReplyAsync("coffee remindme 5m to burn you");
                }

                Console.WriteLine($"Title = {embed.Title}");
                Console.WriteLine($"Description = {embed.Description}");
            }

            if (Context.Message.Author.Username == "CoffeeBot" 
                && Context.Message.Content.Contains("Your reminder from")
                && Context.Message.MentionedUsers.FirstOrDefault()?.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync($"{userMention} Your mother was a hamster and your father smelt of elderberries.");
            }
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Enums;
using GameBot.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        //[Command("heckle")]
        //public async Task Heckle()
        //{
        //    if (Context.Message.Author.Username != "dawiebe") return;
        //    await ReplyAsync("coffee remindme 10m \"create 2 reminders\"");
        //}


        //[Command("hecklecoffeebot")]
        //public async Task HeckleCoffeeBot()
        //{
        //    if (!Context.Message.Author.IsBot) return;

        //    Console.WriteLine(Context.Message.Timestamp);
        //    Console.WriteLine(Context.Message.Author.Username);
        //    Console.WriteLine(Context.Message.Content);

        //    var userMention = Context.Message.Author.Mention;
        //    _phraseService.AddReplacement("<coffeebot>", userMention);

        //    if (Context.Message.Author.Username == "CoffeeBot" && Context.Message.Embeds.Count > 0)
        //    {
        //        var embed = Context.Message.Embeds.First();
        //        if (embed.Title.Contains("Coffee Alert"))
        //        {
        //            //await ReplyAsync(_phraseService.GetPhrase(KeyPhrases.HeckleCoffeeBot));
        //            await Gif(new string[]{ "coffee", "time"});
        //            //await ReplyAsync($"!gif coffee time");
        //        }

        //        Console.WriteLine($"Title = {embed.Title}");
        //        Console.WriteLine($"Description = {embed.Description}");
        //    }

        //    if (Context.Message.Author.Username == "CoffeeBot" 
        //        && Context.Message.Content.Contains("Your reminder from")
        //        && Context.Message.MentionedUsers.FirstOrDefault()?.Id == Context.Client.CurrentUser.Id)
        //    {
        //        var message = Context.Message.Content;
        //        var createIndex = message.IndexOf("create") + 7;
        //        var remindersIndex = message.IndexOf("reminders")-1;
        //        var newNumber = int.Parse(message.Substring(createIndex, remindersIndex - createIndex));

        //        for (int i = 1; i <= newNumber; i++)
        //        {
        //            await ReplyAsync($"coffee remindme 10m \"reminder {i} of {newNumber}\"");
        //            Thread.Sleep(500);
        //        }
        //        await ReplyAsync($"coffee remindme 10m \"create {newNumber*2} reminders\"");
        //        //await ReplyAsync($"{userMention} How does it feel not being able to sort numbers properly?" );
        //        //await ReplyAsync($"[10, 1, 2].sort()");
        //    }
        //}
    }
}

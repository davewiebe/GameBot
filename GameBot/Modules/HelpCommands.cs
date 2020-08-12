using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Enums;
using GameBot.Services;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TenorSharp;
using TenorSharp.ResponseObjects;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Commands:" +
                "\n!new");
        }
    }  
}

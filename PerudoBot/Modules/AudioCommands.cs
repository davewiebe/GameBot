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
        //[Command("join", RunMode = RunMode.Async)]
        //public async Task JoinCmd()
        //{
        //    await _audioService.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        //}


        //[Command("leave", RunMode = RunMode.Async)]
        //public async Task LeaveCmd()
        //{
        //    await _audioService.LeaveAudio(Context.Guild);
        //}

        //[Command("play", RunMode = RunMode.Async)]
        //public async Task PlayCmd([Remainder] string song)
        //{
        //    await _audioService.SendAudioAsync(Context.Guild, Context.Channel, "C:\\Project.Workspace\\GameBot\\asdf.mp3");
        //    await _audioService.SendAudioAsync(Context.Guild, Context.Channel, "C:\\Users\\dawie\\Desktop\\test.mp3");
        //}
    }
}

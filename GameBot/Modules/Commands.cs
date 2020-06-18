using Discord;
using Discord.Commands;
using GameBot.Data;
using GameBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private GameBotDbContext _db;

        public Commands()
        {
            _db = new GameBotDbContext();
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }


        [Command("highscore")]
        public async Task Highscore()
        {
            var monkey = _db.Scores.FirstOrDefault();
            if (monkey == null)
            {
                await ReplyAsync("No scores yet");
                return;
            }
            await ReplyAsync($"Highest score: {monkey.Points}");
        }


        [Command("nothanks")]
        public async Task NoThanks()
        {
            await ReplyAsync("Oh boy");
            Thread.Sleep(500);

            await ReplyAsync("Add users with !add");

        }

        [Command("add")]
        public async Task AddUserToGame()
        {
            await ReplyAsync($"{Context.User.Username} added to game");
        }

        [Command("start")]
        public async Task Start()
        {
            await ReplyAsync($"Starting the game!");


            var deck = new ClassicDeck();
        }
    }
}

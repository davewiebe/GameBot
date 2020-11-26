﻿using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("autoliar")]
        [Alias("auto")]
        [Alias("al")]
        public async Task AutoLiarCallAsync()
        {
            var game = await GetGameAsync(GameState.InProgress);

            var gamePlayer = _db.GamePlayers.AsQueryable()
                    .Include(gp => gp.Player)
                    .Include(gp => gp.GamePlayerRounds)
                    .Where(gp => gp.GameId == game.Id)
                    .Where(x => x.NumberOfDice > 0)
                    .Where(x => x.Player.Username == Context.User.Username)
                    .SingleOrDefault();

            if (gamePlayer == null)
                return;

            gamePlayer.CurrentGamePlayerRound.IsAutoLiarSet = true;

            await SendMessageAsync($":lock: {gamePlayer.Player.Nickname} has locked in a **liar** call.");

            _db.SaveChanges();
        }
    }
}
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Elo;
using PerudoBot.Extensions;
using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class EloRatingService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public EloRatingService(GameBotDbContext db)
        {
            _db = db;
        }

        public enum GameMode
        {
            Variable = 0,
            Standard = 1,
            SuddenDeath = 100,
        }

        public async Task GenerateEloRatingsForAllGamesAsync(ulong guildId, bool erasePreviousRatings = false)
        {
            // todo: call into GenerateEloRatingsForGameAsync rather than duplicating all the logic
            var db = new GameBotDbContext();

            var gameModes = new List<GameMode> { GameMode.Variable, GameMode.SuddenDeath, GameMode.Standard };

            foreach (var gameMode in gameModes)
            {
                var games = await db.Games.AsQueryable()
                    .Include(g => g.GamePlayers)
                        .ThenInclude(gp => gp.Player)
                            .ThenInclude(p => p.EloRatings)
                    .Where(g => g.GuildId == guildId)
                    .Where(g => g.IsRanked && g.State == 3)
                    .Where(g => g.Penalty == (int)(object)gameMode)
                    .Where(g => g.GamePlayers.Count() >= 4)
                    .Where(g => !g.GamePlayers.Any(gp => gp.Rank == null))
                    .OrderBy(g => g.Id)
                    .ToListAsync();

                var ratings = new Dictionary<string, int>();

                games.ForEach(game =>
                {
                    Console.WriteLine($"========================================");
                    Console.WriteLine($"Calculating Elo for game {game.Id}");
                    var match = new EloMatch();

                    foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                    {
                        var nickname = gamePlayer.Player.Nickname;
                        if (!ratings.ContainsKey(nickname))
                        {
                            var currentEloRating = gamePlayer.Player.EloRatings
                                .FirstOrDefault(er => er.GameMode == gameMode.ToString());

                            if (erasePreviousRatings)
                            {
                                currentEloRating.Rating = 1500;
                            }

                            ratings.Add(nickname, currentEloRating?.Rating ?? 1500);
                            gamePlayer.PreGameEloRating = ratings[nickname];
                        }

                        //Console.WriteLine($"Adding {nickname} with {ratings[nickname]} rating and rank of {gamePlayer.Rank}");
                        match.AddPlayer(nickname, gamePlayer.Rank.Value, ratings[nickname]);
                    }

                    match.CalculateElos();

                    foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                    {
                        var nickname = gamePlayer.Player.Nickname;
                        ratings[nickname] = match.GetElo(nickname);

                        gamePlayer.PostGameEloRating = ratings[nickname];

                        var currentEloRating = gamePlayer.Player.EloRatings
                            .FirstOrDefault(er => er.GameMode == gameMode.ToString());

                        if (currentEloRating != null)
                        {
                            currentEloRating.Rating = match.GetElo(nickname);
                        }
                        else
                        {
                            gamePlayer.Player.EloRatings.Add(new EloRating
                            {
                                GameMode = gameMode.ToString(),
                                Rating = match.GetElo(nickname)
                            });
                        }

                        Console.WriteLine($"{nickname} has new rating of {ratings[nickname]} ({match.GetEloChange(nickname)})");
                    }
                });

                var changes = db.SaveChanges();

                Console.WriteLine($"========================================");
                Console.WriteLine($"Final {gameMode} ratings:");

                var i = 1;
                foreach (var rating in ratings.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"{i}: {rating.Key}: {rating.Value}");
                    i++;
                }
                Console.WriteLine($"========================================");
            }
        }

        public async Task GenerateEloRatingsForGameAsync(int gameId)
        {
            var db = new GameBotDbContext();

            var game = await db.Games.AsQueryable()
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                        .ThenInclude(p => p.EloRatings)
                .Where(g => g.IsRanked && g.State == 3)
                .Where(g => g.GamePlayers.Count() >= 4)
                .Where(g => !g.GamePlayers.Any(gp => gp.Rank == null))
                .Where(g => g.Id == gameId)
                .SingleOrDefaultAsync();

            var ratings = new Dictionary<string, int>();
            var gameMode = (GameMode)game.Penalty;

            Console.WriteLine($"========================================");
            Console.WriteLine($"Calculating Elo for game {game.Id}");

            var match = new EloMatch();

            foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
            {
                var nickname = gamePlayer.Player.Nickname;
                if (!ratings.ContainsKey(nickname))
                {
                    var currentEloRating = gamePlayer.Player.EloRatings
                        .FirstOrDefault(er => er.GameMode == gameMode.ToString());

                    ratings.Add(nickname, currentEloRating?.Rating ?? 1500);
                    gamePlayer.PreGameEloRating = ratings[nickname];
                }

                Console.WriteLine($"Adding {nickname} with {ratings[nickname]} rating and rank of {gamePlayer.Rank}");
                match.AddPlayer(nickname, gamePlayer.Rank.Value, ratings[nickname]);
            }

            match.CalculateElos();

            foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
            {
                var nickname = gamePlayer.Player.Nickname;
                ratings[nickname] = match.GetElo(nickname);

                gamePlayer.PostGameEloRating = ratings[nickname];

                var currentEloRating = gamePlayer.Player.EloRatings
                    .FirstOrDefault(er => er.GameMode == gameMode.ToString());

                if (currentEloRating != null)
                {
                    currentEloRating.Rating = match.GetElo(nickname);
                }
                else
                {
                    gamePlayer.Player.EloRatings.Add(new EloRating
                    {
                        GameMode = gameMode.ToString(),
                        Rating = match.GetElo(nickname)
                    });
                }

                Console.WriteLine($"{nickname} has new rating of {ratings[nickname]} ({match.GetEloChange(nickname)})");
            }

            var changes = db.SaveChanges();
            Console.WriteLine($"{changes} changes written");

            Console.WriteLine($"========================================");
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
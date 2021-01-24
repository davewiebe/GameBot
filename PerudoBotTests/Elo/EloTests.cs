using Glicko2;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PerudoBot.Claims;
using PerudoBot.Elo;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PerudoBotTests
{
    [TestFixture]
    public class EloTests
    {
        public enum GameMode
        {
            Variable = 0,
            Standard = 1,
            SuddenDeath = 100,
        }

        [TestCase(GameMode.Standard)]
        [TestCase(GameMode.Variable)]
        [TestCase(GameMode.SuddenDeath)]
        [Test]
        public void GenerateRatings(GameMode gameMode)
        {
            ulong guildId = 689504722163335196;

            var db = new PerudoBot.Data.GameBotDbContext();

            var games = db.Games.AsQueryable()
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                        .ThenInclude(p => p.EloRatings)
                .Where(g => g.GuildId == guildId)
                .Where(g => g.IsRanked && g.State == 3)
                .Where(g => g.Penalty == (int)(object)gameMode)
                .Where(g => g.GamePlayers.Count() > 3)//&& g.GamePlayers.Count() < 8)
                .Where(g => !g.GamePlayers.Any(gp => gp.Rank == null))
                .OrderByDescending(g => g.Id)
                //.Take(5)
                .OrderBy(g => g.Id)
                .ToList();
            //Debug.WriteLine($"========================================");
            //Debug.WriteLine($"Generating Elo Ratings for {gameMode}");

            var ratings = new Dictionary<string, int>();

            games.ForEach(game =>
            {
                Debug.WriteLine($"========================================");
                Debug.WriteLine($"Calculating Elo for game {game.Id}");
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

                    Debug.WriteLine($"Adding {nickname} with {ratings[nickname]} rating and rank of {gamePlayer.Rank}");
                    match.AddPlayer(nickname, gamePlayer.Rank.Value, ratings[nickname]);
                }

                match.CalculateElos();

                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;
                    ratings[nickname] = match.GetElo(nickname);

                    gamePlayer.PostGameEloRating = ratings[nickname];

                    var currentEloRating = gamePlayer.Player.EloRatings
                        .FirstOrDefault(er => er.GameMode == gameMode.ToString()).Rating = match.GetElo(nickname);

                    Debug.WriteLine($"{nickname} has new rating of {ratings[nickname]} ({match.GetEloChange(nickname)})");
                }
            });

            var changes = db.SaveChanges();

            Debug.WriteLine($"========================================");
            Debug.WriteLine($"Final {gameMode} ratings:");

            var i = 1;
            foreach (var rating in ratings.OrderByDescending(x => x.Value))
            {
                Debug.WriteLine($"{i}: {rating.Key}: {rating.Value}");
                i++;
            }
            Debug.WriteLine($"========================================");
        }

        //[TestCase(GameMode.Standard)]
        [TestCase(GameMode.Variable)]
        //[TestCase(GameMode.SuddenDeath)]
        [Test]
        public void GenerateGlicko(GameMode gameMode)
        {
            ulong guildId = 689504722163335196;

            var db = new PerudoBot.Data.GameBotDbContext();

            var games = db.Games.AsQueryable()
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Where(g => g.GuildId == guildId)
                //.Where(g => g.Id >= 81)
                .Where(g => g.IsRanked && g.State == 3)
                .Where(g => g.Penalty == (int)(object)gameMode)
                .Where(g => g.GamePlayers.Count() > 2)//&& g.GamePlayers.Count() < 8)
                .OrderByDescending(g => g.Id)
                //.Take(5)
                .OrderBy(g => g.Id)
                //.Take(2)
                .ToList();
            //Debug.WriteLine($"========================================");
            Debug.WriteLine($"Generating Elo Ratings for {gameMode}");

            var ratings = new Dictionary<string, GlickoRating>();
            var players = new Dictionary<string, GlickoPlayer>();

            games.ForEach(game =>
            {
                var preGameRatings = new Dictionary<string, GlickoPlayer>();
                Debug.WriteLine($"========= GAME {game.Id} ========= ");
                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;
                    if (!ratings.ContainsKey(nickname))
                    {
                        ratings.Add(nickname, new GlickoRating());
                    }

                    if (!players.ContainsKey(nickname))
                    {
                        players.Add(nickname, new GlickoPlayer(ratingDeviation: 80));
                    }

                    //create a copy
                    preGameRatings.Add(nickname, new GlickoPlayer(
                        players[nickname].Rating,
                        players[nickname].RatingDeviation,
                        players[nickname].Volatility)
                    );
                }

                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;
                    Debug.WriteLine($"Adding {nickname} with {preGameRatings[nickname].Rating} rating and rank of {gamePlayer.Rank}");

                    //if (!ratings.ContainsKey(nickname))
                    //{
                    //    ratings.Add(nickname, new GlickoRating());
                    //}

                    var opponents = new List<GlickoOpponent>();

                    foreach (var x in game.GamePlayers.OrderBy(gp => gp.Rank))
                    {
                        var xNick = x.Player.Nickname;
                        if (xNick == nickname) continue;

                        var result = gamePlayer.Rank < x.Rank ? 1 : 0;

                        // Debug.WriteLine($"Adding opponent {xNick} with {preGameRatings[xNick].Rating} rating and rank of {x.Rank} (result: {result}");
                        opponents.Add(new GlickoOpponent(preGameRatings[xNick], result)); //working
                    }

                    players[nickname] = GlickoCalculator.CalculateRanking(players[nickname], opponents);
                    ratings[nickname].PreviousRating = ratings[nickname].Rating;
                    ratings[nickname].Rating = Math.Round(players[nickname].Rating, 3);
                    ratings[nickname].Deviation = Math.Round(players[nickname].RatingDeviation, 3);
                    ratings[nickname].Volatility = Math.Round(players[nickname].Volatility, 3);
                    ratings[nickname].GamesPlayed++;
                }

                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;

                    //gamePlayer.EloRatingChange = match.GetEloChange(nickname);

                    Debug.WriteLine($"{nickname} ({gamePlayer.Rank}) has new rating of {ratings[nickname].Rating} ({ratings[nickname].RatingChange}) and dev of {ratings[nickname].Deviation}");
                }
            });

            //var changes = db.SaveChanges();

            Debug.WriteLine($"========================================");
            Debug.WriteLine($"Final {gameMode} ratings:");

            var i = 1;
            foreach (var rating in ratings.OrderByDescending(x => x.Value.Rating))
            {
                Debug.WriteLine($"{i}: {rating.Key}: {Math.Round(rating.Value.Rating)} | {rating.Value.GamesPlayed}GP | φ: {Math.Round(rating.Value.Deviation)} | σ: {rating.Value.Volatility}");
                i++;
            }
            //Debug.WriteLine($"========================================");

            //Debug.WriteLine(String.Format("Player ranking: {0}", dave.Rating));
            //Debug.WriteLine(String.Format("Player ranking deviation: {0}", dave.RatingDeviation));

            //dave = GlickoCalculator.CalculateRanking(dave, player1Opponents);

            //Debug.WriteLine(String.Format("Player ranking: {0}", dave.Rating));
            //Debug.WriteLine(String.Format("Player ranking deviation: {0}", dave.RatingDeviation));
        }

        public class GlickoRating
        {
            private double _rating = 1500;
            public double PreviousRating { get; set; }
            public double RatingChange => Math.Round(Rating - PreviousRating, 3);

            public double Rating { get => _rating; set => _rating = (value >= 0 ? value : 0); }
            public double Deviation { get; set; }
            public double Volatility { get; set; }
            public int GamesPlayed { get; set; } = 0;
        }
    }
}
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
                .Where(g => g.GuildId == guildId)
                .Where(g => g.Id >= 81)
                .Where(g => g.IsRanked && g.State == 3)
                .Where(g => g.Penalty == (int)(object)gameMode)
                //.Where(g => g.GamePlayers.Count() > 8)//&& g.GamePlayers.Count() < 8)
                .OrderByDescending(g => g.Id)
                //.Take(5)
                .OrderBy(g => g.Id)
                .ToList();
            //Debug.WriteLine($"========================================");
            //Debug.WriteLine($"Generating Elo Ratings for {gameMode}");

            var ratings = new Dictionary<string, int>();

            games.ForEach(game =>
            {
                //Debug.WriteLine($"========================================");
                //Debug.WriteLine($"Calculating Elo for game {game.Id}");
                var match = new EloMatch();

                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;
                    if (!ratings.ContainsKey(nickname))
                    {
                        ratings.Add(nickname, 1500);
                        //switch (gameMode)
                        //{
                        //    case GameMode.Variable:
                        //        ratings.Add(nickname, gamePlayer.Player.EloRatingVariable);
                        //        break;

                        //    case GameMode.SuddenDeath:
                        //        ratings.Add(nickname, gamePlayer.Player.EloRatingSuddenDeath);
                        //        break;

                        //    case GameMode.Standard:
                        //        ratings.Add(nickname, gamePlayer.Player.EloRatingStandard);
                        //        break;

                        //    default:
                        //        break;
                        //}
                    }
                    //Debug.WriteLine($"Adding {nickname} with {ratings[nickname]} rating and rank of {gamePlayer.Rank}");
                    match.AddPlayer(nickname, gamePlayer.Rank.Value, ratings[nickname]);
                }

                match.CalculateElos();

                foreach (var gamePlayer in game.GamePlayers.OrderBy(gp => gp.Rank))
                {
                    var nickname = gamePlayer.Player.Nickname;
                    ratings[nickname] = match.GetElo(nickname);
                    gamePlayer.EloRatingChange = match.GetEloChange(nickname);

                    switch (gameMode)
                    {
                        case GameMode.Variable:
                            gamePlayer.Player.EloRatingVariable = match.GetElo(nickname);
                            break;

                        case GameMode.SuddenDeath:
                            gamePlayer.Player.EloRatingSuddenDeath = match.GetElo(nickname);
                            break;

                        case GameMode.Standard:
                            gamePlayer.Player.EloRatingStandard = match.GetElo(nickname);
                            break;

                        default:
                            break;
                    }
                    //Debug.WriteLine($"{nickname} has new rating of {ratings[nickname]} ({match.GetEloChange(nickname)})");
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
    }
}
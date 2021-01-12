using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using PerudoBot.Extensions;
using PerudoBot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class PerudoGameService : IDisposable
    {
        public readonly GameBotDbContext _db;

        public PerudoGameService(GameBotDbContext db)
        {
            _db = db;
        }

        public async Task TerminateGameAsync(int gameId)
        {
            var gameToTerminate = await _db.Games.AsQueryable()
                .SingleAsync(g => g.Id == gameId);

            if (gameToTerminate.State != (int)GameState.Finished)
            {
                gameToTerminate.State = (int)GameState.Terminated;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Game> GetGameAsync(ulong channelId, params GameState[] gameStates)
        {
            var gameStateIds = gameStates.Cast<int>().ToList();

            return await _db.Games.AsQueryable()
                .Include(g => g.Rounds)
                    .ThenInclude(r => r.Actions)
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Where(x => x.ChannelId == channelId)
                .Where(x => gameStateIds.Contains(x.State))
                .SingleOrDefaultAsync();
        }

        public async Task UpdateGamePlayerRanksAsync(int gameId)
        {
            var game = await _db.Games.AsQueryable()
                .Include(g => g.GamePlayers).ThenInclude(gp => gp.Player)
                .Include(g => g.Rounds).ThenInclude(r => r.GamePlayerRounds)
                .Where(g => g.Id == gameId)
                .SingleAsync();

            var rounds = game.Rounds.OrderBy(r => r.RoundNumber);

            var currentRank = game.GamePlayers.Count();

            foreach (var round in game.Rounds)
            {
                var playersEliminated = round.GamePlayerRounds
                    .Where(gpr => gpr.IsEliminated)
                    .Where(gpr => !gpr.IsGhost)
                    .Select(gpr => gpr.GamePlayer).ToList();

                foreach (var gamePlayer in playersEliminated)
                {
                    gamePlayer.Rank = currentRank;
                    currentRank--;
                }
            }

            var winningGamePlayer = game.GamePlayers
                .Where(gp => gp.Player.Username == game.Winner)
                .FirstOrDefault();

            if (winningGamePlayer != null)
            {
                winningGamePlayer.Rank = 1;
            }

            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }


        public List<string> GetOptions(Game game)
        {
            var options = new List<string>();

            if (game.NumberOfDice < 10)
            {
                var dice = "";
                if (game.NumberOfDice >= 1) dice += ":one:";
                if (game.NumberOfDice >= 2) dice += ":two:";
                if (game.NumberOfDice >= 3) dice += ":three:";
                if (game.NumberOfDice >= 4) dice += ":four:";
                if (game.NumberOfDice >= 5) dice += ":five:";
                if (game.NumberOfDice >= 6) dice += ":six:";
                if (game.NumberOfDice >= 7) dice += ":seven:";
                if (game.NumberOfDice >= 8) dice += ":eight:";
                if (game.NumberOfDice >= 9) dice += ":nine:";

                options.Add(dice);
            }
            else
                options.Add($":game_die: ˣ `{game.NumberOfDice}`");

            if (game.Penalty == 0)
            {
                if (game.NumberOfDice < 10)
                {
                    var fire = "";
                    if (game.NumberOfDice >= 1) fire += ":candle:";
                    if (game.NumberOfDice >= 2) fire += ":grey_question:";
                    if (game.NumberOfDice >= 3) fire += ":grey_question:";
                    if (game.NumberOfDice >= 4) fire += ":grey_question:";
                    if (game.NumberOfDice >= 5) fire += ":grey_question:";
                    if (game.NumberOfDice >= 6) fire += ":grey_question:";
                    if (game.NumberOfDice >= 7) fire += ":grey_question:";
                    if (game.NumberOfDice >= 8) fire += ":grey_question:";
                    if (game.NumberOfDice >= 9) fire += ":grey_question:";
                    options.Add(fire);
                }
                else
                    options.Add($":grey_question: ˣ `{game.NumberOfDice}`");
            }
            else
            {
                if (game.NumberOfDice < 10)
                {
                    var penalty = System.Math.Min(game.NumberOfDice, game.Penalty);
                    var fire = "";
                    if (penalty >= 1) fire += ":candle:";
                    if (penalty >= 2) fire += ":candle:";
                    if (penalty >= 3) fire += ":candle:";
                    if (penalty >= 4) fire += ":candle:";
                    if (penalty >= 5) fire += ":candle:";
                    if (penalty >= 6) fire += ":candle:";
                    if (penalty >= 7) fire += ":candle:";
                    if (penalty >= 8) fire += ":candle:";
                    if (penalty >= 9) fire += ":candle:";
                    options.Add(fire);
                }
                else
                    options.Add($":candle: ˣ `{game.NumberOfDice}`");
            }
            if (game.LowestPip != 1 || game.HighestPip != 6) options.Add($"{game.LowestPip.GetEmoji()} :left_right_arrow: {game.HighestPip.GetEmoji()}");

            // remove this option?? if (game.RandomizeBetweenRounds) options.Add("Player order will be **randomized** between rounds");
            if (!game.WildsEnabled) options.Add(":x: :one:");

            if (!game.Palifico) options.Add($":x: :game_die:");

            if (!game.FaceoffEnabled) options.Add(":x: :face_with_monocle:");

            if (game.ExactCallBonus > 0 || game.ExactCallPenalty > 0) options.Add($":white_check_mark: :dart: `{game.ExactCallBonus}`:shield: `{game.ExactCallPenalty}`:crossed_swords:");
            else options.Add($":x: :dart: :twisted_rightwards_arrows:");

            if (game.CanCallLiarAnytime) options.Add(":white_check_mark: :lying_face: :twisted_rightwards_arrows:");
            else options.Add(":x: :lying_face: :twisted_rightwards_arrows:");

            if (game.CanBidAnytime) options.Add(":white_check_mark: :tickets: :twisted_rightwards_arrows:");

            if (game.CanCallExactToJoinAgain) options.Add(":white_check_mark: :ghost: :ghost: :ghost:");
            else options.Add(":x: :ghost: ");

            if (game.IsRanked) options.Add(":white_check_mark: :medal:");
            else options.Add(":x: :medal:");

            if (game.PenaltyGainDice) options.Add(":white_check_mark: :upside_down:");

            if (game.TerminatorMode) options.Add(":white_check_mark: :robot:");
            return options;
        }
        public List<GamePlayer> GetGamePlayers(Game game)
        {
        return _db.GamePlayers.AsQueryable()
            .Include(gp => gp.Player)
            .Include(gp => gp.GamePlayerRounds)
            .Where(gp => gp.GameId == game.Id)
            .OrderBy(x => x.TurnOrder)
            .ToList();
        }


        public void AddUserToGame(Game game, SocketGuildUser user)
        {
            if (user == null)
            {
                throw new Exception("Can't add user. They aren't online");
            }

            // TODO: Can't add players if they aren't online (or found in cache)
            bool userAlreadyExistsInGame = UserAlreadyExistsInGame(user.Username, game);
            if (userAlreadyExistsInGame)
            {
                return;
            }

            // get player
            // TODO: replace Username with UserId lookup when all user Ids are populated
            var player = _db.Players.AsQueryable()
                .Where(p => p.GuildId == game.GuildId)
                .Where(p => p.Username == user.Username)
                .FirstOrDefault();

            if (player != null) // update player
            {
                player.Nickname = user.Nickname ?? user.Username;
                player.UserId = user.Id;
            }
            else // create a new player
            {
                player = new Player
                {
                    Username = user.Username,
                    Nickname = user.Nickname ?? user.Username,
                    GuildId = user.Guild.Id,
                    UserId = user.Id,
                    IsBot = user.IsBot
                };
            }

            _db.GamePlayers.Add(new GamePlayer
            {
                Game = game,
                Player = player
            });

            _db.SaveChanges();
        }

        private bool UserAlreadyExistsInGame(string username, Game game)
        {
            var players = GetGamePlayers(game);
            bool userAlreadyExistsInGame = players.FirstOrDefault(x => x.Player.Username == username) != null;
            return userAlreadyExistsInGame;
        }
    }
}
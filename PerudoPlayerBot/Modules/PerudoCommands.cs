using Discord.Commands;
using Discord.WebSocket;
using PerudoPlayerBot.Data;
using PerudoPlayerBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoPlayerBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("perudo")]
        public async Task Perudo()
        {
            if (IsFromPerudoBot(Context.Message)) return;

            MessageData message = _messageParser.Parse(Context.Message);


            if (message.MessageType == MessageTypes.StartGame)
            {
                _gameService.DeactivatePreviousGames();

                _db.Games.Add(new Game() { Active = true });
                _db.SaveChanges();
                return;
            }

            if (message.MessageType == MessageTypes.NewDice)
            {
                await ReplyAsync($"Got my dice: {string.Join(", ", message.MyDice)}");

                SaveDice(Context.Client.CurrentUser.Username, message.MyDice);
                return;
            }

            if (message.MessageType == MessageTypes.PlayerCall)
            {
                // save call
                // end round
            }

            if (message.MessageType == MessageTypes.PlayerBid)
            {
                // save bid

                //if (message.MessageType == MessageTypes.MyTurn)
                //{
                //    calculate
                //}
            }


            if (message.MessageType == MessageTypes.RoundSummary)
            {
                SaveRoundSummary();
                return;
            }

            if (message.MessageType == MessageTypes.CurrentStandings)
            {
                SaveCurrentStandings();
                return;
            }
        }

        private void SaveCurrentStandings()
        {
            throw new NotImplementedException();
        }

        private void SaveRoundSummary()
        {
            throw new NotImplementedException();
        }

        private bool IsFromPerudoBot(SocketUserMessage message)
        {
            if (message.Author.Username != _perudoBotUsername) return false;
            return true;
        }

        private void SaveDieCount(string username, int numberOfDice)
        {
            var lastRound = _gameService.AddRoundIfNotExists();
            var player = _playerService.AddPlayerIfNotExists(username);

            var playerRound = new PlayerRound()
            {
                NumberOfDice = numberOfDice,
                Player = player,
                Round = lastRound
            };

            _db.PlayerRound.Add(playerRound);
            _db.SaveChanges();
        }

        private void SaveDice(string username, List<int> dice)
        {
            var currentRound = _gameService.AddRoundIfNotExists();
            var player = _playerService.AddPlayerIfNotExists(username);

            var playerRound = new PlayerRound()
            {
                Dice = string.Join(",", dice),
                NumberOfDice = dice.Count,
                Player = player,
                Round = currentRound
            };

            _db.PlayerRound.Add(playerRound);
            _db.SaveChanges();
        }

    }
}

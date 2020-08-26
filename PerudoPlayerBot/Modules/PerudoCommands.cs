using Discord.Commands;
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
            MessageData message = _messageParser.Parse(Context.Message);

            //if (!message.MessageIsFromPerudoBot()) return;

            //if (_messageParser.MessageIsStartGame())
            //{
            //    _gameService.DeactivatePreviousGames();

            //    _db.Games.Add(new Game() { Active = true });
            //    _db.SaveChanges();
            //    return;
            //}

            //if (_messageParser.MessageIsNewDice())
            //{
            //    var dice = _messageParser.GetDice();
            //    await ReplyAsync($"Got my dice: {string.Join(", ", dice)}");

            //    var botUsername = Context.Client.CurrentUser.Username;

            //    SaveDice(botUsername, dice);
            //    return;
            //}

            //if (_messageParser.MessageIsPlayerCall())
            //{
            //    // save call
            //    // end round
            //}

            //if (_messageParser.MessageIsPlayerBid())
            //{
            //    // save call
            //}

            //if (_messageParser.MessageIsMyTurn())
            //{
            //    // calculate
            //}

            //if (_messageParser.MessageIsRoundSummary())
            //{
            //    SaveRoundSummary();
            //    return;
            //}

            //if (_messageParser.MessageIsCurrentStandings())
            //{
            //    SaveCurrentStandings();
            //    return;
            //}
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

        private void SaveRoundSummary()
        {
            var message = Context.Message.Content;
            var userDiceCsv = message.Substring(message.IndexOf("Dice: "));
            var userDiceList = userDiceCsv.Split(",").Select(x => x.Trim());
            foreach (var userDice in userDiceList)
            {
                var username = userDice.Substring(0, userDice.IndexOf(": "));
                var diceEmojis = userDice.Substring(userDice.IndexOf(": ")).Split(" ");

                var dice = _messageParser.GetDiceFromEmojiList(diceEmojis);

                // Todo, break this function into parser service + round service
                SaveDice(username, dice);
            }
        }

        private void SaveCurrentStandings()
        {
            //var usernameDiceList = _messageParser.GetUsernamesAndDiceInGame();
            //_playerService.CreatePlayersThatDontExist(usernameDiceList.Select(x => x.Username).ToList());

            //foreach (var usernameDice in usernameDiceList)
            //{
            //    SaveDieCount(usernameDice.Username, usernameDice.DiceCount);
            //}
        }

        //private bool MessageIsForMe()
        //{
        //    var userMention = Context.Message.MentionedUsers.FirstOrDefault();
        //    if (userMention == null) return false;

        //    var perudoPlayerBot = Context.Client.CurrentUser;
        //    if (userMention.Id != perudoPlayerBot.Id) return false;

        //    return true;
        //}
    }
}

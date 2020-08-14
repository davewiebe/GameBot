using Discord.Commands;
using Discord.WebSocket;
using PerudoPlayerBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PerudoPlayerBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("perudo")]
        public async Task Perudo()
        {
            if (!MessageIsFromPerudoBot()) return;

            if (MessageIsStartGame())
            {
                DeactivatePreviousGames();

                _db.Games.Add(new Data.Game() { Active = true });
                _db.SaveChanges();
            }

            if (MessageIsNewDice())
            {
                var dice = GetDice();
                await ReplyAsync($"Got my dice: {string.Join(",", dice)}");

                if (IsFirstRound())
                {
                    // add new round
                }

                // Add dice

                return;
            }

            if (MessageIsPlayerBid())
            {

            }

            if (MessageIsMyTurn())
            {

            }

            if (MessageIsDiceSummary())
            {
                SaveMyDice();
            }

            if (MessageIsCurrentStandings())
            {
                SaveCurrentStandings();
            }
        }

        private void SaveMyDice()
        {
            throw new NotImplementedException();
        }

        private bool IsFirstRound()
        {
            throw new NotImplementedException();
        }

        private void DeactivatePreviousGames()
        {
            var activeGames = _db.Games.AsQueryable().Where(x => x.Active == true);
            foreach (var activeGame in activeGames)
            {
                activeGame.Active = false;
            }
            _db.SaveChanges();
        }

        private bool MessageIsStartGame()
        {
            return Context.Message.Content.StartsWith("Starting the game");
        }

        private bool MessageIsDiceSummary()
        {
            return Context.Message.Content.StartsWith("Dice: ");
        }

        private void SaveCurrentStandings()
        {
            var summary = Context.Message.Embeds.Where(x => x.Title == "Current standings");
            var stuff = summary.First().Fields.First().Value;
            var listOfUsersAndDice = stuff.Split("\n");

            //var players = _db.Players.AsQueryable().Where(x => x.Round.Active == true).ToList();

            foreach (var userAndDie in listOfUsersAndDice)
            {
                if (string.IsNullOrEmpty(userAndDie)) continue;
                if (userAndDie.Contains("Total dice left:")) continue;

                var items = userAndDie.Split(" ");
                var dice = int.Parse(items[0].Trim('`'));
                var username = userAndDie.Substring(userAndDie.IndexOf(" "));

                // create and save 
                //var player = players.SingleOrDefault(x => x.Username == username);

                //if (player == null)
                //{
                    //var newPlayer = new Player()
                    //{
                        
                    //}
                //}
                //if (!players.FirstOrDefault(x => x.Username == username))
                //{

                //}
                //_db.Players.Add()
            }
        }

        private bool MessageIsCurrentStandings()
        {
            var embeds = Context.Message.Embeds;
            if (embeds == null) return false;

            var summary = embeds.Where(x => x.Title == "Current standings");
            if (summary.Count() == 0) return false;

            return true;
        }

        private bool MessageIsMyTurn()
        {
            return Context.Message.Content.Contains($"{Context.Client.CurrentUser.Mention} is up");
        }
        private bool MessageIsPlayerBid()
        {
            var containsTimes = Context.Message.Content.Contains($"ˣ");
            var containsIsUp = Context.Message.Content.Contains($"is up");
            return (containsTimes && containsIsUp);
        }
        

        private List<int> GetDice()
        {
            var startIndex = Context.Message.Content.IndexOf("||")+2;
            var temp = Context.Message.Content.Substring(startIndex);
            var cypherText = temp.Substring(0, temp.Length - 2);
            var dice = SimpleAES.AES256.Decrypt(cypherText, _aesEncryptionKey);
            return dice.Split().Select(x => int.Parse(x)).ToList();
        }

        private bool MessageIsNewDice()
        {
            return Context.Message.Content.Contains($"{Context.Client.CurrentUser.Mention}'s dice:");
        }

        private bool MessageIsFromPerudoBot()
        {
            if (Context.Message.Author.Username != _perudoBotUsername) return false;
            return true;
        }

        private bool MessageIsForMe()
        {
            var userMention = Context.Message.MentionedUsers.FirstOrDefault();
            if (userMention == null) return false;

            var perudoPlayerBot = Context.Client.CurrentUser;
            if (userMention.Id != perudoPlayerBot.Id) return false;

            return true;
        }
    }
}

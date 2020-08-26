using Discord.Commands;
using Discord.WebSocket;
using PerudoPlayerBot.Data;
using System.Collections.Generic;
using System.Linq;

namespace PerudoPlayerBot.Services
{
    public class MessageParserService
    {
        private string _perudoBotUsername;
        private string _aesEncryptionKey;

        public MessageParserService(string perudoBotUsername, string aesEncryptionKey)
        {
            _perudoBotUsername = perudoBotUsername;
            _aesEncryptionKey = aesEncryptionKey;
        }

        internal MessageData Parse(SocketUserMessage message)
        {
            return new MessageData()
            {
                MessageType = GetMessageType(message)
            };
        }

        public MessageTypes GetMessageType(SocketUserMessage message)
        {
            if (MessageIsStartGame(message)) return MessageTypes.StartGame;
            if (MessageIsRoundSummary(message)) return MessageTypes.RoundSummary;
            if (MessageIsCurrentStandings(message)) return MessageTypes.CurrentStandings;
            if (MessageIsPlayerBid(message)) return MessageTypes.PlayerBid;
            if (MessageIsPlayerCall(message)) return MessageTypes.PlayerCall;

            return MessageTypes.Unknown;
        }


        private bool MessageIsStartGame(SocketUserMessage message)
        {
            return message.Content.StartsWith("Starting the game");
        }

        private bool MessageIsRoundSummary(SocketUserMessage message)
        {
            return message.Content.StartsWith("Dice: ");
        }

        public List<int> GetDiceFromEmojiList(string[] diceEmojis)
        {
            var dice = new List<int>();
            foreach (var dieEmoji in diceEmojis)
            {
                if (dieEmoji == ":one:") dice.Add(1);
                if (dieEmoji == ":two:") dice.Add(2);
                if (dieEmoji == ":three:") dice.Add(3);
                if (dieEmoji == ":four:") dice.Add(4);
                if (dieEmoji == ":five:") dice.Add(5);
            }

            return dice;
        }
        public List<int> GetDice(SocketUserMessage message)
        {
            var startIndex = message.Content.IndexOf("||") + 2;
            var temp = message.Content.Substring(startIndex);
            var cypherText = temp.Substring(0, temp.Length - 2);
            var dice = SimpleAES.AES256.Decrypt(cypherText, _aesEncryptionKey);
            return dice.Split().Select(x => int.Parse(x)).ToList();
        }

        public bool MessageIsMyTurn()
        {
            // ToDO; Redo function as "who's turn is next"
            return false;
            //return _context.Message.Content.Contains($"{_context.Client.CurrentUser.Mention} is up");
        }
        private bool MessageIsPlayerBid(SocketUserMessage message)
        {
            var containsTimes = message.Content.Contains($"ˣ");
            var containsIsUp = message.Content.Contains($"is up");
            return (containsTimes && containsIsUp);
        }

        private bool MessageIsPlayerCall(SocketUserMessage message)
        {
            var liarCall = message.Content.Contains($"called **liar** on");
            var exactCall = message.Content.Contains($"called **exact** on");
            return (liarCall || exactCall);
        }

        private bool MessageIsNewDice(SocketUserMessage message)
        {
            //TODO: does this even work???
            return message.Content.Contains($"'s dice:");
        }

        public bool MessageIsFromPerudoBot(SocketUserMessage message)
        {
            if (message.Author.Username != _perudoBotUsername) return false;
            return true;
        }
        private bool MessageIsCurrentStandings(SocketUserMessage message)
        {
            var embeds = message.Embeds;
            if (embeds == null) return false;

            var summary = embeds.Where(x => x.Title == "Current standings");
            if (summary.Count() == 0) return false;

            return true;
        }
        public class UsernameDice
        {
            public string Username { get; set; }
            public int DiceCount { get; set; }
        }

        public List<UsernameDice> GetUsernamesAndDiceInGame(SocketUserMessage message)
        {
            var summary = message.Embeds.Where(x => x.Title == "Current standings");
            var stuff = summary.First().Fields.First().Value;
            var listOfUsersAndDice = stuff.Split("\n");

            var usernameDiceList = new List<UsernameDice>();

            foreach (var userAndDie in listOfUsersAndDice)
            {
                if (string.IsNullOrEmpty(userAndDie)) continue;
                if (userAndDie.Contains("Total dice left:")) continue;

                var items = userAndDie.Split(" ");
                var dice = int.Parse(items[0].Trim('`'));
                var username = userAndDie.Substring(userAndDie.IndexOf(" "));

                usernameDiceList.Add(new UsernameDice() { Username = username, DiceCount = dice });
            }

            return usernameDiceList;
        }
    }
}

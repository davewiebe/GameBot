using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using PerudoPlayerBot.Data;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace PerudoPlayerBot.Services
{
    public class MessageParserService
    {
        private string _aesEncryptionKey;

        public MessageParserService(string aesEncryptionKey)
        {
            _aesEncryptionKey = aesEncryptionKey;
        }

        internal MessageData Parse(SocketUserMessage message)
        {
            var messageData = new MessageData()
            {
                MessageType = GetMessageType(message)
            };

            switch (messageData.MessageType)
            {
                case MessageTypes.StartGame:
                    break;
                case MessageTypes.RoundSummary:
                    break;
                case MessageTypes.PlayerBid:
                    break;
                case MessageTypes.NewDice:
                    messageData.MyDice = GetDice(message);
                    break;
                case MessageTypes.Unknown:
                    break;
                case MessageTypes.PlayerCall:
                    break;
                case MessageTypes.CurrentStandings:
                    messageData.CurrentStandings = GetUsernamesAndDiceInGame(message);
                    break;
                default:
                    break;
            }

            return messageData;
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
            return message.Content.StartsWith("Round summary for bots:");
        }

        public List<int> GetDice(SocketUserMessage message)
        {
            var cypherText = message.Content.Split("||")[1];
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

        private bool MessageIsCurrentStandings(SocketUserMessage message)
        {
            return message.Content.StartsWith("Current standings for bots:");
        }


        public class Player
        {
            public string Username { get; set; }
            public int DiceCount { get; set; }
        }

        public class CurrentStandingsDto
        {
            public List<Player> Players { get; set; }
            public int TotalPlayers { get; set; }
            public int TotalDice { get; set; }
        }

        public List<Player> GetUsernamesAndDiceInGame(SocketUserMessage message)
        {
            var jsonMessage = message.Content.Split("||")[1];
            var currentStandings = JsonConvert.DeserializeObject<CurrentStandingsDto>(jsonMessage);
            return currentStandings.Players;
        }
    }
}

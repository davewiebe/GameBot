using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoPlayerBot.Services
{
    class MessageData
    {
        public MessageTypes MessageType { get; internal set; }
        public List<int> MyDice { get; internal set; }
        public List<MessageParserService.Player> CurrentStandings { get; internal set; }
    }

    public enum MessageTypes
    {
        StartGame,
        RoundSummary,
        PlayerBid,
        NewDice,
        Unknown,
        PlayerCall,
        CurrentStandings
    }
}

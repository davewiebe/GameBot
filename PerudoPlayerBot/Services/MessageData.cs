using System;
using System.Collections.Generic;
using System.Text;

namespace PerudoPlayerBot.Services
{
    class MessageData
    {
        public bool IsStartGame { get; internal set; }
        public MessageTypes MessageType { get; internal set; }
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

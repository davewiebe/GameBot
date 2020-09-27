using System;
using System.Collections.Generic;

namespace PerudoBot.Data
{
    public class Game
    {
        public int Id { get; set; }
        public int State { get; set; }
        
        public int? PlayerTurnId { get; set; }
        public int NumberOfDice { get; internal set; }
        public int Penalty { get; internal set; }
        public bool RandomizeBetweenRounds { get; internal set; }
        public bool WildsEnabled { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public int RoundStartPlayerId { get; internal set; }
        public int ExactCallBonus { get; internal set; }
        public bool CanCallExactAnytime { get; internal set; }
        public bool CanCallLiarAnytime { get; internal set; }
        public int ExactCallPenalty { get; internal set; }
        public bool CanBidAnytime { get; internal set; }
        public bool Palifico { get; internal set; }
        public bool NextRoundIsPalifico { get; internal set; }
        public bool IsRanked { get; internal set; }
        public DateTime DateCreated { get; internal set; }
        public ulong GuildId { get; internal set; }
        public string Winner { get; internal set; }
        public bool FaceoffEnabled { get; internal set; }

        public virtual List<Note> Notes { get; set; }
        public DateTime DateFinished { get; internal set; }
        public bool CanCallExactToJoinAgain { get; internal set; }
        public ulong StatusMessage { get; internal set; }
    }
}

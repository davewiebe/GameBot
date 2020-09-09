using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameBot.Data
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
    }

    public class Note
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
        public string Text { get; set; }
        public string Username { get; internal set; }
    }

    public class Player
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }
        //public ulong UserId { get; set; }
        public string Username { get; set; }
        public int NumberOfDice { get; set; }
        public string Dice { get; set; }
        public int TurnOrder { get; internal set; }
        public bool IsBot { get; internal set; }
    }

    public class BotKey
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string BotAesKey { get; set; }
    }

    public class Bid
    {
        public int Id { get; set; }
        public virtual Player Player { get; set; }
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public string Call { get; set; }
    }
}

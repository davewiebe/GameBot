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

using System.ComponentModel.DataAnnotations.Schema;

namespace GameBot.Data
{
    public class Game
    {
        public int Id { get; set; }
        public int State { get; set; }
        
        public int? PlayerTurnId { get; set; }
    }
    public class Player
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }
        public string Username { get; set; }
        public int NumberOfDice { get; set; } = 5;
        public int? Die1 { get; set; }
        public int? Die2 { get; set; }
        public int? Die3 { get; set; }
        public int? Die4 { get; set; }
        public int? Die5 { get; set; }
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

using System.ComponentModel.DataAnnotations.Schema;

namespace GameBot.Data
{
    public class Game
    {
        public int Id { get; set; }
        public int State { get; set; }
        public virtual Player PlayerTurn { get; set; }
        public int PlayerTurnId { get; set; }
    }
    public class Player
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }
        public string Username { get; set; }
        public string Dice { get; set; }
    }
    public class Bid
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }
        public virtual Player Player { get; set; }
        public int PlayerId { get; set; }
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public string Call { get; set; }
    }
}

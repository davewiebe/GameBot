namespace PerudoPlayerBot.Data
{
    public class Game
    {
        public int Id { get; set; }
        public bool Active { get; set; }
    }
    public class Bid
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public virtual Round Round { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public bool IsExact { get; set; }
        public bool IsLiar { get; set; }
    }
    public class Round
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }
        public bool Active { get; set; }
    }
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
    public class PlayerRound
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public int RoundId { get; set; }
        public virtual Round Round { get; set; }
        public int NumberOfDice { get; set; }
        public string Dice { get; set; }

    }
}
namespace PerudoBot.Data
{
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
        public int GhostAttemptsLeft { get; internal set; }
        public int GhostAttemptQuantity { get; internal set; }
        public int GhostAttemptPips { get; internal set; }
    }
}

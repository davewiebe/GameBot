using System.ComponentModel.DataAnnotations.Schema;

namespace PerudoBot.Data
{
    public class GamePlayer
    {
        public int Id { get; set; }
        public virtual Game Game { get; set; }
        public int GameId { get; set; }

        // TODO: Make not nullable after migration
        public int? PlayerId { get; set; }

        public virtual Player Player { get; set; }
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
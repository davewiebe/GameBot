using System.Collections.Generic;
using System.ComponentModel;

namespace PerudoBot.Data
{
    public class GamePlayer
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public virtual Player Player { get; set; }

        public int GameId { get; set; }

        public virtual Game Game { get; set; }

        public virtual ICollection<Action> Actions { get; set; }

        public int NumberOfDice { get; set; }
        public string Dice { get; set; }
        public int TurnOrder { get; internal set; }

        public int GhostAttemptsLeft { get; internal set; }
        public int GhostAttemptQuantity { get; internal set; }
        public int GhostAttemptPips { get; internal set; }

        public int? Place { get; set; }
    }
}
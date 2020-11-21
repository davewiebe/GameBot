using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.Data
{
    public class GamePlayerRound
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public Round Round { get; set; }

        public int GamePlayerId { get; set; }
        public GamePlayer GamePlayer { get; set; }

        public int NumberOfDice { get; set; }
        public string Dice { get; set; }

        public int TurnOrder { get; set; }
        public bool IsGhost { get; set; }
        public bool WasEliminated { get; set; }
        public int Penalty { get; set; }
    }
}
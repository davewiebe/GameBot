using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Data
{
    public abstract class Round
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int RoundNumber { get; set; }
        public virtual Game Game { get; set; }
        public ICollection<Action> Actions { get; set; }
        public ICollection<RoundPlayer> RoundPlayers { get; set; }

        public int StartingPlayerId { get; set; }

        public string RoundType { get; private set; }

        public Action GetLatestAction() => Actions.LastOrDefault();
    }

    public class StandardRound : Round
    {
    }

    public class PalificoRound : Round
    {
    }

    public class FaceoffRound : Round
    {
    }
}
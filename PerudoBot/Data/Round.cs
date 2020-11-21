using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Data
{
    public abstract class Round
    {
        public Round()
        {
            DateStarted = DateTime.Now;
        }

        public int Id { get; set; }
        public int GameId { get; set; }
        public int RoundNumber { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateFinished { get; set; }

        public virtual Game Game { get; set; }
        public ICollection<Action> Actions { get; set; }
        public ICollection<GamePlayerRound> GamePlayerRounds { get; set; }

        public int StartingPlayerId { get; set; }

        public string RoundType { get; private set; }

        public Action LatestAction => Actions.LastOrDefault();

        public Action GetLatestAction() => Actions.LastOrDefault();

        public double DurationInSeconds => (DateFinished - DateStarted).TotalSeconds;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PerudoBot.Data
{
    public abstract class Action
    {
        public Action()
        {
            TimeStamp = DateTime.Now;
        }

        public int Id { get; set; }

        public virtual GamePlayer GamePlayer { get; set; }

        public int GamePlayerId { get; set; }

        public virtual GamePlayerRound GamePlayerRound { get; set; }

        // has to be nullable as there's no way to assign historical data
        public int? GamePlayerRoundId { get; set; }

        public int RoundId { get; set; }
        public Round Round { get; set; }

        public int? ParentActionId { get; set; }

        [ForeignKey("ParentActionId")]
        public virtual Action ParentAction { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsOutOfTurn { get; set; }
        public string ActionType { get; private set; }
        public DateTime TimeStamp { get; set; }
        public double? DurationInSeconds { get; set; }

        public void SetDuration()
        {
            if (ParentAction == null && ParentActionId != null)
                throw new Exception("Parent Action must be loaded to calculate duration");

            if (ParentAction != null)
            {
                DurationInSeconds = (TimeStamp - ParentAction.TimeStamp).TotalSeconds;
            }
            else
            {
                DurationInSeconds = (TimeStamp - Round.DateStarted).TotalSeconds;
            }
        }
    }

    public class Bid : Action
    {
        public int Quantity { get; set; }
        public int Pips { get; set; }
        public ulong MessageId { get; internal set; }
    }

    public class LiarCall : Action
    {
    }

    public class ExactCall : Action
    {
    }
}